namespace FoodRecommendation.Constant
{
    public class Enumeration
    {
        public enum ToastType : int
        {
            None = 0,
            Success = 1,
            Error = 2,
            Warning = 3,
        }

        public enum EditMode : int
        {
            Add = 1,
            Edit = 2,
            Delete = 3,
        }

        public enum RecipeStatus : int
        {
            Archived = 0, // Luu tru
            Pending = 1, // Chờ duyệt
            Approved = 2, // Duyệt
            Rejected = 3 // Từ Chối

        }


    }
}
