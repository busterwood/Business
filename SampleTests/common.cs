using System;
    

    internal interface IProcess<TStep, TContext> 
        where TStep : struct, IConvertible // enum
        where TContext : class
    {
        TStep Step { get; set; }
        TContext Context { get; set; }

    }

    internal class Transition<TStep, TContext> : IDisposable
        where TStep : struct, IConvertible // enum
        where TContext : class
    {
        IProcess<TStep, TContext> process;
        TStep step;
        TContext context;
        bool complete;

        public Transition(IProcess<TStep, TContext> process)
        {
            this.process = process;
            this.step = process.Step;
            this.context = process.Context;
            this.complete = false;
        }

        public void Complete()
        {
            complete = true;
        }

        public void Dispose()
        {
            // rollback changes if not complete
            if (!complete)
            {
                process.Context = this.context;
                process.Step = this.step;
            }
        }
    }
