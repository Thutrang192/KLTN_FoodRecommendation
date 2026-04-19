from fastapi import FastAPI
from sentence_transformers import SentenceTransformer
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
import uvicorn
from data_loader import get_data_from_sql
app = FastAPI()

# Khởi tạo Model và Data khi bật Server
print("Đang tải AI Model và Dữ liệu...")
model_sbert = SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')
df_recipes = get_data_from_sql()

# Tính sẵn Vector TF-IDF và SBERT cho toàn bộ kho dữ liệu
tfidf = TfidfVectorizer()
tfidf_matrix = tfidf.fit_transform(df_recipes['Content'])
sbert_embeddings = model_sbert.encode(df_recipes['Content'].tolist())

@app.get("/recommend/{recipe_id}")
def recommend(recipe_id: int):
    if recipe_id not in df_recipes['RecipeId'].values:
        return []

    # Tìm vị trí (index) của món ăn trong DataFrame
    idx = df_recipes.index[df_recipes['RecipeId'] == recipe_id][0]

    # Tính độ tương đồng TF-IDF
    tfidf_sim = cosine_similarity(tfidf_matrix[idx], tfidf_matrix).flatten()

    # Tính độ tương đồng SBERT
    current_embedding = sbert_embeddings[idx].reshape(1, -1)
    sbert_sim = cosine_similarity(current_embedding, sbert_embeddings).flatten()

    # Hybrid Score
    final_score = (tfidf_sim * 0.4) + (sbert_sim * 0.6)

    # Lấy Top 4 món ăn liên quan nhất (bỏ qua chính nó)
    df_recipes['temp_score'] = final_score
    related = df_recipes[df_recipes['RecipeId'] != recipe_id].nlargest(4, 'temp_score')

    return related['RecipeId'].tolist()

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)