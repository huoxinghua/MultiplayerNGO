using System;

namespace _Project.Code.Utilities.ServiceLocator
{
    public interface IService : IDisposable
    {
        void Initialize();
    }
}