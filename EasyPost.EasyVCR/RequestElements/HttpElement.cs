using EasyPost.EasyVCR.InternalUtilities.JSON;

namespace EasyPost.EasyVCR.RequestElements
{
    public class HttpElement
    {
        /// <summary>
        ///     Serialize this object to a JSON string
        /// </summary>
        /// <returns>JSON string representation of this HttpInteraction object.</returns>
        internal string ToJson()
        {
            return Serialization.ConvertObjectToJson(this);
        }
    }
}
