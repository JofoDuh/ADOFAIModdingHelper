using System.Threading.Tasks;
using ThunderKit.Core.Pipelines;

namespace Plugins.Script Symbol Definition
{
    [PipelineSupport(typeof(Pipeline))]
    public class  : PipelineJob
    {
        public override Task Execute(Pipeline pipeline)
        {
            return Task.CompletedTask;
        }
    }
}
