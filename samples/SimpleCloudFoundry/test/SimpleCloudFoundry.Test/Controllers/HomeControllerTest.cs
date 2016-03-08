﻿using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.OptionsModel;

using SimpleCloudFoundry.Controllers;
using Xunit;

namespace SimpleCloudFoundry.Test.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public void CloudFoundry_IOptionsNull()
        {
            HomeController controller = new HomeController(null, null, null, null);
            var result = controller.CloudFoundry() as ViewResult;
            Assert.Equal(0, result.ViewData.Count);
        }
    }
    class TestOptions<T> : IOptions<T> where T : class, new()
    {
        public TestOptions(T value)
        {
            Value = value;
        }
        public T Value { get; set; }

    }
}