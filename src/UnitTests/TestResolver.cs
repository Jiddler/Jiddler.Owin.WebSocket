using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace UnitTests {
    class TestResolver : IServiceLocator {
        public Dictionary<Type, object> Types = new Dictionary<Type, object>();

        public object GetService(Type serviceType) {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType) {
            object t;
            if (!Types.TryGetValue(serviceType, out t)) {
                t = Activator.CreateInstance(serviceType);
                Types.Add(serviceType, t);
            }

            return t;
        }

        public object GetInstance(Type serviceType, string key) {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType) {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>() {
            object t;
            if (!Types.TryGetValue(typeof(TService), out t)) {
                t = Activator.CreateInstance<TService>();
                Types.Add(typeof(TService), t);
            }

            return (TService) t;
        }

        public TService GetInstance<TService>(string key) {
            throw new NotImplementedException();
        }

        public IEnumerable<TService> GetAllInstances<TService>() {
            throw new NotImplementedException();
        }
    }
}