using Spring.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleCloudFoundry4.ViewModels
{
    public class CloudFoundryViewModel
    {
        public CloudFoundryViewModel(CloudFoundryApplicationOptions appOptions, CloudFoundryServicesOptions servOptions)
        {
            CloudFoundryServices = servOptions;
            CloudFoundryApplication = appOptions;
        }
        public CloudFoundryServicesOptions CloudFoundryServices { get; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; }
    }
}