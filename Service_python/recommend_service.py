import numpy as np
import pickle
import re
from sklearn.metrics.pairwise import cosine_similarity
from deep_translator import GoogleTranslator
from underthesea import word_tokenize

# =========================
# LOAD DATA
# =========================
with open("recipe_vectors.pkl", "rb") as f:
    data = pickle.load(f)

recipe_ids = data["RecipeId"]
recipe_titles = data["Title"]
recipe_vectors = data["recipe_vectors"]
recipe_ingredients = data["IngredientCore"]

tfidf = data["tfidf"]
model = data["sbert_model"]

print("Loaded recipes:", len(recipe_ids))
print("Vector shape:", recipe_vectors.shape)

# =========================
# NORMALIZATION MAP
# =========================
CANONICAL_MAP = {
    "thịt_heo": "thịt_lợn",
    "thịt_ba_chỉ": "thịt_lợn",
    "thịt_heo_xay": "thịt_lợn",
    "thịt_ba_rọi": "thịt_lợn",
    "thịt_xay": "thịt_lợn",
    "thịt_nạc": "thịt_lợn",
    "trứng_gà": "trứng",
    "trứng_vịt": "trứng",
    "cá_hồi_phi_lê": "cá_hồi",
    "cá_hồi_tươi": "cá_hồi"
}

def normalize_ingredient(ing):
    ing = ing.strip().lower().replace(" ", "_")
    return CANONICAL_MAP.get(ing, ing)

# =========================
# UNITS + MODIFIERS
# =========================
units = [
    "g","gram","kg","ml","l","lit","mg",
    "muỗng","muỗng_canh","muỗng_cà_phê",
    "thìa","thìa_canh","thìa_cà_phê",
    "cup","tsp","tbsp",
    "bát","chén","ly",
    "miếng","lát"
]

modifier_words = [
    "củ","quả","trái","nhánh",
    "ít","nhiều","vừa","khoảng",
    "băm","cắt","thái","xắt",
    "nhỏ","lớn",
    "tươi","non","già"
]

SEASONINGS = {
    "muối","đường","hạt_nêm","tiêu","nước_mắm","hành_tím",
    "dầu_ăn","dầu_hào","bột_ngọt","tỏi","hành","hành_lá",
    "hành_khô","ớt","gừng","sả","gia_vị"
}

# =========================
# TRANSLATE CACHE
# =========================
translator_cache = {}

def translate_token(token):
    if token in translator_cache:
        return translator_cache[token]

    try:
        if all(ord(c) < 128 for c in token):
            token = GoogleTranslator(source='en', target='vi').translate(token)
    except:
        pass

    translator_cache[token] = token
    return token

# =========================
# NORMALIZE TEXT
# =========================
def normalize_text(text):
    text = str(text).lower()

    text = text.replace("\u200b", " ")
    text = re.sub(r"\(.*?\)", "", text)

    # remove numbers
    text = re.sub(r"\d+\/\d+|\d+", " ", text)

    text = re.sub(r"[^\w\s,:;]", " ", text)

    text = re.sub(r"\b(và|với)\b", ",", text)

    text = re.sub(r"\s+", " ", text)

    return text.strip()

# =========================
# PREPROCESS USER INPUT
# =========================
def preprocess_user_input(user_input_list):

    processed = []

    for item in user_input_list:

        item = translate_token(item.strip())
        item = normalize_text(item)

        tokens = word_tokenize(item)

        tokens = [
            w for w in tokens
            if w not in units
            and w not in modifier_words
            and not re.match(r"\d+", w)
        ]

        clean = " ".join(tokens)

        if clean:
            processed.append(clean)

    return processed

# =========================
# REMOVE SEASONINGS
# =========================
def remove_seasonings(ings):
    core = [i for i in ings if i not in SEASONINGS]
    return core if len(core) > 0 else ings

# =========================
# MAIN RECOMMEND FUNCTION
# =========================
def recommend_ids(user_input, top_k=10):

    # ---------- PREPROCESS ----------
    user_input = preprocess_user_input(user_input)
    user_input = [normalize_ingredient(x) for x in user_input]

    user_core = remove_seasonings(user_input)
    if len(user_core) == 0:
        user_core = user_input

    user_text = " ".join(user_core)

    # ---------- VECTORIZE ----------
    user_tfidf = tfidf.transform([user_text]).toarray()
    user_sbert = model.encode([user_text])

    user_vector = np.hstack((user_tfidf, user_sbert))

    # ---------- COSINE SIM ----------
    sims = cosine_similarity(user_vector, recipe_vectors)[0]

    results = []

    for i, sim in enumerate(sims):

        recipe_ing = [normalize_ingredient(x) for x in recipe_ingredients[i]]

        user_set = set(user_input)
        recipe_set = set(recipe_ing)

        matched = user_set & recipe_set
        missing = recipe_set - user_set

        coverage = len(matched) / len(recipe_set) if recipe_set else 0
        missing_ratio = len(missing) / len(recipe_set) if recipe_set else 0

        score = 0.7 * sim + 0.2 * coverage - 0.1 * missing_ratio

        results.append((recipe_ids[i], score))

    # ---------- FILTER BAD RESULTS ----------
    results = [r for r in results if r[1] >= 0.2]

    # ---------- SORT ----------
    results.sort(key=lambda x: x[1], reverse=True)

    # ---------- RETURN ----------
    return [r[0] for r in results[:top_k]]