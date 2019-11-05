﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Jens.RestServer
{
    internal class ExposedRestServerService
    {
        public Type ServiceType { get; set; }
        public Type TopLevelInterfaceType { get; set; }
        public RestServerServiceInstanceType InstanceType { get; set; } = RestServerServiceInstanceType.Instance;
        public IRestServerService SingletonInstance { get; set; }
        public List<ExposedRestServerAction> Routes { get; set; } = new List<ExposedRestServerAction>();

        private readonly IRestServerDependencyResolver _restServerDependencyResolver;

        public ExposedRestServerService(IRestServerDependencyResolver restServerDependencyResolver)
        {
            _restServerDependencyResolver = restServerDependencyResolver ?? throw new ArgumentNullException(nameof(restServerDependencyResolver));
        }

        public object GetInstance(HttpListenerContext context)
        {
            if (InstanceType == RestServerServiceInstanceType.Instance)
            {
                return ApplyContext(InternalCreateInstance(), context);
            }

            if (InstanceType == RestServerServiceInstanceType.SingletonStrict ||
                InstanceType == RestServerServiceInstanceType.SingletonLazy)
            {
                if (SingletonInstance == null)
                {
                    SingletonInstance = InternalCreateInstance();
                }

                return ApplyContext(SingletonInstance, context);
            }

            throw new NotImplementedException($"{nameof(ExposedRestServerService)}: InstanceType not implemented. {InstanceType}");
        }

        private IRestServerService InternalCreateInstance()
        {
            var instance = _restServerDependencyResolver.Activate(ServiceType) as IRestServerService;

            if (instance == null)
                throw new InvalidOperationException($"Could not get dependency {ServiceType.FullName}.");

            return instance;
        }

        private static IRestServerService ApplyContext(IRestServerService RestServerService, HttpListenerContext context)
        {
            if (RestServerService == null)
            {
                throw new ArgumentNullException(nameof(RestServerService));
            }

            RestServerService.Request = context?.Request;
            RestServerService.Response = context?.Response;

            return RestServerService;
        }
    }
}

