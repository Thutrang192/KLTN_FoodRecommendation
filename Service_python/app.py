from fastapi import FastAPI
from pydantic import BaseModel
from typing import List

from recommend_service import recommend_ids

app = FastAPI()

# =========================
# REQUEST MODEL
# =========================
class RecommendRequest(BaseModel):
    ingredients: List[str]
    top_k: int = 10

# =========================
# RESPONSE API
# =========================
@app.post("/recommend")
def recommend(req: RecommendRequest):

    recipe_ids = recommend_ids(req.ingredients, req.top_k)

    return {
        "recipe_ids": recipe_ids
    }

# =========================
# HEALTH CHECK
# =========================
@app.get("/")
def home():
    return {"status": "Food Recommendation API running"}