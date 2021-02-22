using System.Threading.Tasks;
using vtb.Messaging.Context;

namespace vtb.Messaging.Pipelines
{
    public delegate Task FilterDelegate(IMessageContext messageContext);
}