using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XStatic.Generator.Storage;

namespace XStatic.Generator.Transformers
{
    public class DefaultHtmlTransformerListFactory : ITransformerListFactory
    {
        public virtual IEnumerable<ITransformer> BuildTransformers(ISiteConfig siteConfig) {
            if (!string.IsNullOrEmpty(siteConfig.TargetHostname)) {
                yield return new HostnameTransformer(siteConfig.TargetHostname);
            }
        }
    }
}