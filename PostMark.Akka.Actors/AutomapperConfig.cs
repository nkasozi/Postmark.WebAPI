using AutoMapper;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostMark.Akka.Actors
{
    public static class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.CreateMap<SingleEmail, SingleEmailResult>().ReverseMap();
            Mapper.CreateMap<BulkEmail, BulkEmailResult>()
               .ForMember
                (
                   dest => dest.Results,
                   opt => opt.MapFrom(src => src.Emails)
                )
                .ReverseMap();

            
        }
    }
}
