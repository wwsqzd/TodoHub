

using AutoMapper;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<CreateTodoDTO, TodoEntity>();
            CreateMap<UpdateTodoDTO, TodoEntity>();
            CreateMap<TodoEntity, TodoDTO>();

            CreateMap<UserDTO, UserEntity>().ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
            CreateMap<UserEntity, UserDTO>();
            CreateMap<RegisterDTO, UserEntity>();
        }
    }
}
