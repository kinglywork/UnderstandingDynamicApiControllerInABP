using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace TestCastleWindsorProxy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = new WindsorContainer();
            container.Register(
                Component
                    .For(typeof(ITaskAppService))
                    .ImplementedBy(typeof(TaskAppService))
            );
            container.Register(
                Component
                    .For(typeof(DynamicApiControllerInterceptor<ITaskAppService>))
            );
            container.Register(
                Component.For(typeof(DynamicApiController))
                    .Proxy.AdditionalInterfaces(typeof(ITaskAppService))
                    .Interceptors(typeof(DynamicApiControllerInterceptor<ITaskAppService>))
            );

            var dynamicApiControllerInstance = container.Resolve<DynamicApiController>();

            dynamicApiControllerInstance.Execute();
            (dynamicApiControllerInstance as ITaskAppService)?.UpdateTask(1);

            Console.ReadKey();
        }
    }

    public interface ITaskAppService
    {
        void UpdateTask(int id);
    }

    public class TaskAppService : ITaskAppService
    {
        public void UpdateTask(int id)
        {
            Console.WriteLine("TaskAppService.UpdateTask: {0}", id);
        }
    }

    public class DynamicApiController
    {
        public void Execute()
        {
            Console.WriteLine("DynamicApiController.Execute");
        }
    }

    public class DynamicApiControllerInterceptor<T> : IInterceptor
    {
        private readonly T _proxiedObject;

        public DynamicApiControllerInterceptor(T proxiedObject)
        {
            _proxiedObject = proxiedObject;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = invocation.Method.Invoke(_proxiedObject, invocation.Arguments);
        }
    }
}