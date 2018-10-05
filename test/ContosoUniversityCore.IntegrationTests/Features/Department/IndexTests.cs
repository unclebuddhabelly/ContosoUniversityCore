﻿using System.Linq;

namespace ContosoUniversityCore.IntegrationTests.Features.Department
{
    using System;
    using System.Threading.Tasks;
    using ContosoUniversityCore.Features.Department;
    using Domain;
    using Shouldly;
    using Xunit;
    using static SliceFixture;

    public class IndexTests : IntegrationTestBase
    {
        [Fact]
        public async Task Should_list_departments()
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
            var dept2 = new Department
            {
                Name = "English",
                Administrator = admin,
                Budget = 456m,
                StartDate = DateTime.Today
            };

            await InsertAsync(dept, dept2);

            var query = new Index.Query();

            var result = await SendAsync(query);

            result.ShouldNotBeNull();
            result.Count.ShouldBeGreaterThanOrEqualTo(2);
            result.Select(m => m.Id).ShouldContain(dept.Id);
            result.Select(m => m.Id).ShouldContain(dept2.Id);
        }

    }
}