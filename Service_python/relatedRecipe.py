from sklearn.metrics.pairwise import cosine_similarity

def recommend_recipe(recipe_id, df_recipes, tfidf_matrix, sbert_embeddings):
    recipe_id = str(recipe_id)
    df_recipes['RecipeId'] = df_recipes['RecipeId'].astype(str)
    
    exists = recipe_id in df_recipes['RecipeId'].values
    print(f"DEBUG: Tìm thấy ID trong DB: {exists}")

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
