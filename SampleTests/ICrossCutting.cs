using System;

namespace SampleTests
{
    interface ICrossCutting<T> where T : struct, IConvertible
    {
        void OnEnd(T step);
        void OnFailure(T step, Exception e);
        void OnStart(T step);
    }
}