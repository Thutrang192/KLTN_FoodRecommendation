using AutoMapper;
using FoodRecommendation.Models.Entity;
using FoodRecommendation.Models.Model;

namespace FoodRecommendation.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Account, RegisterModel>();
            CreateMap<RegisterModel, Account>();
        }
    }
}
