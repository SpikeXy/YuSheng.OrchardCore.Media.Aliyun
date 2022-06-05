using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace YuSheng.OrchardCore.Media.AliYun.Filters
{

    public class ResizeOssUrlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
            string text = input.ToStringValue();
            bool flag = !text.Contains("?");
            if (flag)
            {
                text += "?";
            }
            FluidValue fluidValue = arguments["narrow"].Or(arguments.At(0));
            bool flag2 = !fluidValue.IsNil();
            if (flag2)
            {
                text = text + "&x-oss-process=image/auto-orient,1/resize,p_" + fluidValue.ToStringValue() + "/quality,q_90";
            }
            return new ValueTask<FluidValue>(new StringValue(text));
        }
    }

}
