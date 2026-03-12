using AutoMapper;
using KhoaCNTT.Application.DTOs;
using KhoaCNTT.Application.DTOs.Admin;
using KhoaCNTT.Application.DTOs.File;
using KhoaCNTT.Application.DTOs.Lecturer;
using KhoaCNTT.Domain.Entities;
using KhoaCNTT.Domain.Entities.FileEntities;

namespace KhoaCNTT.Application.Common.Utils
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Cấu hình map 2 chiều: Từ Entity -> DTO và ngược lại
            CreateMap<FileResource, FileRequestDto>().ReverseMap();
            CreateMap<FileRequest, FileRequestDto>()
                .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.NewResource.Admin.FullName))
                .ForMember(dest => dest.NewFileName, opt => opt.MapFrom(src => src.NewResource.FileName))
                .ForMember(dest => dest.NewFileSize, opt => opt.MapFrom(src => src.NewResource.Size))
                .ForMember(dest => dest.OldFileName, opt => opt.MapFrom(src => src.OldResource != null ? src.OldResource.FileName : null))
                .ForMember(dest => dest.OldFileSize, opt => opt.MapFrom(src => src.OldResource != null ? src.OldResource.Size : (long?)null));
            CreateMap<Admin, AdminResponse>();

            CreateMap<Lecturer, LecturerResponse>()
                .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src =>
                    src.LecturerSubjects.Select(ls => new SubjectBriefDto
                    {
                        SubjectCode = ls.SubjectCode,
                        SubjectName = ls.Subject != null ? ls.Subject.SubjectName : ""
                    }).ToList()));
            //CreateMap<News, NewsDto>().ReverseMap();
            //CreateMap<Lecture, LectureDto>().ReverseMap();
            //CreateMap<Comment, CommentDto>().ReverseMap();
        }
    }
}