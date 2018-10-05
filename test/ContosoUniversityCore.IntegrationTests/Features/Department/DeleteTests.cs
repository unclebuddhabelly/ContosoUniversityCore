﻿using System.Linq;

namespace ContosoUniversityCore.IntegrationTests.Features.Department
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using ContosoUniversityCore.Features.Department;
    using Domain;
    using Shouldly;
    using Xunit;
    using static SliceFixture;

    public class DeleteTests : IntegrationTestBase
    {
        [Fact]
        public async Task Should_delete_department()
        {
            var adminId = await SendAsync(new ContosoUniversityCore.Features.Instructor.CreateEdit.Command
            {
                FirstMidName = "George",
                LastName = "Costanza",
                HireDate = DateTime.Today,
            });
            var admin = await FindAsync<Instructor>(adminId);

            var dept = new Department
            {
                Name = "History",
                Administrator = admin,
                Budget = 123m,
                StartDate = DateTime.Today
            };
            await InsertAsync(dept);

            var command = new Delete.Command
            {
                Id = dept.Id,
                RowVersion = dept.RowVersion
            };

            await SendAsync(command);

            var any = await ExecuteDbContextAsync(db => db.Departments.Where(d => d.Id == command.Id).AnyAsync());

            any.ShouldBeFalse();
        }
    }
}