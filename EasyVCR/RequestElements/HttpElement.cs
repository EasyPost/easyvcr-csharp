using EasyVCR.InternalUtilities.JSON;

namespace EasyVCR.RequestElements
{
    /// <summary>
    ///     Base class for all EasyVCR request/response objects.
    /// </summary>
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
