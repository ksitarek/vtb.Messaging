using System;
using System.Collections.Generic;
using System.Linq;

namespace vtb.Messaging.Pipelines
{
    public class PipelineBuilder
    {
        private readonly LinkedList<Type> _filterTypes;
        public IEnumerable<Type> FilterTypes => _filterTypes.ToList();

        public static PipelineBuilder Create()
        {
            return new PipelineBuilder();
        }

        protected PipelineBuilder()
        {
            _filterTypes = new LinkedList<Type>();
        }

        public PipelineBuilder AddFilter<TFilter>()
        {
            _filterTypes.AddLast(typeof(TFilter));
            return this;
        }

        public Pipeline Build(IServiceProvider serviceProvider)
        {
            var filters = _filterTypes.Select(ft => serviceProvider.GetService(ft) as IFilter);
            return new Pipeline(new LinkedList<IFilter>(filters));
        }
    }
}