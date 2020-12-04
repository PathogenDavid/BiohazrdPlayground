using Biohazrd;
using Biohazrd.Transformation;

namespace BiohazrdPlayground
{
    internal sealed class DllImportNameTransformation : TransformationBase
    {
        protected override TransformationResult TransformFunction(TransformationContext context, TranslatedFunction declaration)
            => declaration with
            {
                DllFileName = "Example.dll"
            };

        protected override TransformationResult TransformStaticField(TransformationContext context, TranslatedStaticField declaration)
            => declaration with
            {
                DllFileName = "Example.dll"
            };
    }
}
