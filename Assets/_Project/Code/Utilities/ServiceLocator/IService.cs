using System;

namespace _Project.Code.Core.ServiceLocator
{
    public interface IService : IDisposable
    {
        void Initialize();
    }
}