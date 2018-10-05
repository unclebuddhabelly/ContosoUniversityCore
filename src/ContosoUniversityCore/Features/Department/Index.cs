﻿namespace ContosoUniversityCore.Features.Department
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using AutoMapper;
    using Infrastructure;
    using MediatR;

    public class Index
    {
        public class Query : IRequest<List<Model>>
        {
        }

        public class Model
        {
            public string Name { get; set; }

            public decimal Budget { get; set; }

            public DateTime StartDate { get; set; }

            public int Id { get; set; }

            [Display(Name = "Administrator")]
            public string AdministratorFullName { get; set; }
        }

        public class QueryHandler : AsyncRequestHandler<Query, List<Model>>
        {
            private readonly SchoolContext _context;

            public QueryHandler(SchoolContext context)
            {
                _context = context;
            }

            protected override async Task<List<Model>> HandleCore(Query message)
            {
                return await _context.Departments
                  .ProjectToListAsync<Model>();
            }
        }
    }
}