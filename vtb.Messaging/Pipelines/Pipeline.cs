using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using vtb.Messaging.Context;

namespace vtb.Messaging.Pipelines
{

    public sealed class Pipeline
    {
        private readonly LinkedList<IFilter> _filters;

        public Pipeline(LinkedList<IFilter> filters)
        {
            _filters = filters;
        }

        public Task Run(IMessageContext messageContext, FilterDelegate last)
        {
            return BuildFunc(_filters.First, last).Invoke(messageContext);
        }

        private FilterDelegate BuildFunc(LinkedListNode<IFilter> node, FilterDelegate last)
        {
            var next = default(FilterDelegate);
            if (node.Next == null)
            {
                next = last;
            }
            else
            {
                next = BuildFunc(node.Next, last);
            }

            return (messageContext) => node.Value.Invoke(messageContext, next);
        }
    }
}