import pandas as pd
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from sentence_transformers import SentenceTransformer
from deep_translator import GoogleTranslator

# =========================
# 1 Load datasets
# =========================

df_ingredients = pd.read_csv("ingredients_clean_detail.csv")
df_recipes = pd.read_csv("recipes.csv")

recipes = df_ingredients.groupby("RecipeId")["IngredientProcessed"].apply(list).reset_index()
recipes = recipes.merge(df_recipes, on="RecipeId", how="left")

# =========================
# 2 Ingredient Canonical Map
# =========================

CANONICAL_MAP = {

    "thịt_heo": "thịt_lợn",
    "thịt_ba_chỉ": "thịt_lợn",
    "thịt_xay": "thịt_lợn",
    "thịt_nạc": "thịt_lợn",

    "trứng_gà": "trứng",
    "trứng_vịt": "trứng",

    "cá_hồi_phi_lê": "cá_hồi",
    "cá_hồi_tươi": "cá_hồi"
}

def normalize_ingredient(ing):

    if ing in CANONICAL_MAP:
        return CANONICAL_MAP[ing]

    return ing

# normalize dataset ingredients
recipes["IngredientProcessed"] = recipes["IngredientProcessed"].apply(
    lambda x: [normalize_ingredient(i) for i in x]
)

recipes["ingredient_text"] = recipes["IngredientProcessed"].apply(lambda x: " ".join(x))

# =========================
# 3 Seasoning list
# =========================

SEASONINGS = {
    "muối","đường","hạt_nêm","tiêu","nước_mắm",
    "dầu_ăn","dầu_hào","bột_ngọt","tỏi","hành",
    "hành_khô","ớt","gừng","sả", "gia_vị"
}

# =========================
# 4 TF-IDF
# =========================

tfidf = TfidfVectorizer()
tfidf_matrix = tfidf.fit_transform(recipes["ingredient_text"]).toarray()

# =========================
# 5 SBERT
# =========================

model = SentenceTransformer("keepitreal/vietnamese-sbert")

sbert_matrix = model.encode(
    recipes["ingredient_text"],
    show_progress_bar=True
)

recipe_vectors = np.hstack((tfidf_matrix, sbert_matrix))

# =========================
# 6 Translator cache
# =========================

translator_cache = {}

def translate_token_if_english(token):

    if token in translator_cache:
        return translator_cache[token]

    if all(ord(c) < 128 for c in token):
        try:
            vi = GoogleTranslator(source='en', target='vi').translate(token)
        except:
            vi = token
    else:
        vi = token

    translator_cache[token] = vi
    return vi

# =========================
# 7 Coverage calculation
# =========================

def coverage_match(user_input, recipe_ings):

    user_set = set(user_input)

    # remove seasoning
    core_recipe = [i for i in recipe_ings if i not in SEASONINGS]

    if len(core_recipe) == 0:
        core_recipe = recipe_ings

    recipe_set = set(core_recipe)

    matched = user_set & recipe_set
    missing = recipe_set - user_set

    coverage = len(matched) / len(recipe_set)
    missing_ratio = len(missing) / len(recipe_set)

    return coverage, list(missing), missing_ratio, list(matched)

# =========================
# 8 Recommend
# =========================

def recommend(user_input, top_k=5):

    # translate input
    user_input = [translate_token_if_english(x.strip()) for x in user_input]

    # replace space
    user_input = [x.replace(" ", "_") for x in user_input]

    # normalize ingredient
    user_input = [normalize_ingredient(x) for x in user_input]

    # vector user
    user_text = " ".join(user_input)

    user_tfidf = tfidf.transform([user_text]).toarray()
    user_sbert = model.encode([user_text])

    user_vector = np.hstack((user_tfidf, user_sbert))

    sims = cosine_similarity(user_vector, recipe_vectors)[0]

    results = []

    for i, sim in enumerate(sims):

        recipe_ing = recipes.iloc[i]["IngredientProcessed"]

        coverage, missing, missing_ratio, matched = coverage_match(
            user_input,
            recipe_ing
        )

        score = 0.7 * sim + 0.2 * coverage - 0.1 * missing_ratio

        results.append({
            "RecipeId": recipes.iloc[i]["RecipeId"],
            "RecipeName": recipes.iloc[i]["Title"],
            "Ingredients": recipe_ing,
            "Coverage": coverage,
            "Missing": missing,
            "Matched": matched,
            "Score": score
        })

    results = sorted(results, key=lambda x: x["Score"], reverse=True)

    return results[:top_k]

# =========================
# 9 User loop
# =========================

print("\n===== HỆ GỢI Ý MÓN ĂN =====")

while True:

    text = input("\nNhập nguyên liệu: ")

    if text.lower() == "exit":
        break

    user_input = [x.strip() for x in text.split(",")]

    recs = recommend(user_input)

    print("\n===== GỢI Ý =====\n")

    for r in recs:

        print(f"Recipe: {r['RecipeName']}")
        print(f"Ingredients: {', '.join(r['Ingredients'])}")
        print(f"Matched: {', '.join(r['Matched'])}")
        print(f"Missing: {', '.join(r['Missing'])}")
        print(f"Coverage: {round(r['Coverage'],3)}")
        print(f"Score: {round(r['Score'],3)}")
        print("---------------------------------\n")