using System.Threading.Tasks;
using vtb.Messaging.Context;

namespace vtb.Messaging.Pipelines
{

    public interface IFilter
    {
        Task Invoke(IMessageContext messageContext, FilterDelegate next);
    }
}