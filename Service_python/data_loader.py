import pyodbc
import pandas as pd

def get_data_from_sql():
    # Cấu hình chuỗi kết nối (giống appsettings.json)
    conn_str = (
        "Driver={SQL Server};"
        "Server=LAPTOP-QQ5PR77M;"
        "Database=FoodRecommendation;"
        "Trusted_Connection=yes;"
    )
    
    conn = pyodbc.connect(conn_str)

    query = """
    SELECT 
        r.RecipeId, 
        r.Title,
        (SELECT STRING_AGG(IngredientsText, ' ') FROM Ingredients WHERE RecipeId = r.RecipeId) as AllIngredients,
        (SELECT STRING_AGG(StepText, ' ') FROM Steps WHERE RecipeId = r.RecipeId) as AllSteps
    FROM Recipes r
    WHERE r.IsDeleted = 0 AND r.RecipeStatus = 2
    """

    df = pd.read_sql(query, conn)
    conn.close()
    
    df['Content'] = (
        df['Title'].fillna('') + " " + 
        df['AllIngredients'].fillna('') + " " + 
        df['AllSteps'].fillna('')
    ).str.lower()

    return df