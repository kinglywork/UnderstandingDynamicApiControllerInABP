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
                Component.For(typeof(IStudentAppService)).ImplementedBy(typeof(StudentAppService)).LifestyleTransient(),
                Component.For(typeof(DynamicApiControllerInterceptor<IStudentAppService>)).LifestyleTransient(),
                Component.For(typeof(DynamicApiController<IStudentAppService>))
                    .Proxy.AdditionalInterfaces(typeof(IStudentAppService))
                    .Interceptors(typeof(DynamicApiControllerInterceptor<IStudentAppService>))
                    .LifestyleTransient()
            );

            var dynamicApiControllerInstance = container.Resolve<DynamicApiController<IStudentAppService>>();

            var methodInfoInActionDescripter = dynamicApiControllerInstance.GetType().GetMethod("GetTask");
            var student = methodInfoInActionDescripter?.Invoke(dynamicApiControllerInstance, new object[] {1});
        }
    }

    public class Student
    {
        public int Id { get; set; }

        public Student(int id)
        {
            Id = id;
        }
    }

    public interface IStudentAppService
    {
        Student GetTask(int id);
    }

    public class StudentAppService : IStudentAppService
    {
        public Student GetTask(int id)
        {
            return new Student(1);
        }
    }

    public class DynamicApiController<T>
    {
        public void Execute()
        {
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