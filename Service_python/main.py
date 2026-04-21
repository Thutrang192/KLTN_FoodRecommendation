from fastapi import FastAPI
from sentence_transformers import SentenceTransformer
from sklearn.feature_extraction.text import TfidfVectorizer
import uvicorn
from data_loader import get_data_from_sql

from relatedRecipe import recommend_recipe

app = FastAPI()

print("Đang tải AI Model và Dữ liệu...")

model_recommend = SentenceTransformer('keepitreal/vietnamese-sbert')
df_recipes = get_data_from_sql().reset_index(drop=True)

tfidf = TfidfVectorizer()
tfidf_matrix = tfidf.fit_transform(df_recipes['Content'])
sbert_embeddings = model_recommend.encode(df_recipes['Content'].tolist())

@app.get("/recommend/{recipe_id}")
def recommend(recipe_id: int):
    return recommend_recipe(
        recipe_id,
        df_recipes,
        tfidf_matrix,
        sbert_embeddings
    )

# RUN SERVER
if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)