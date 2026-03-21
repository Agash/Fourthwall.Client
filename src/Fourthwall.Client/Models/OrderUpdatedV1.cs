using Fourthwall.Client.Generated.Models.Openapi.Model.OrderV1;
using Microsoft.Kiota.Abstractions.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Kiota-compatible model for the <c>order.updated</c> webhook payload (<c>OrderUpdatedV1</c>).
/// This type is defined in the OpenAPI spec's <c>components/schemas</c> section but was not emitted
/// by Kiota because no REST endpoint returns it directly; only the <c>order.updated</c> webhook
/// definition references it. The <see cref="Order"/> property reuses the generated <see cref="OrderV1"/>
/// type so the same object graph is shared with REST API responses.
/// </summary>
public sealed class OrderUpdatedV1 : IAdditionalDataHolder, IParsable
{
    /// <inheritdoc />
    public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

    /// <summary>Gets or sets the full updated order.</summary>
    public OrderV1? Order { get; set; }

    /// <summary>Gets or sets the descriptor identifying what changed in this update.</summary>
    public OrderUpdatedV1Update? Update { get; set; }

    /// <summary>Creates a new instance from a parse node (discriminator factory).</summary>
    public static OrderUpdatedV1 CreateFromDiscriminatorValue(IParseNode parseNode)
    {
        ArgumentNullException.ThrowIfNull(parseNode);
        return new OrderUpdatedV1();
    }

    /// <inheritdoc />
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() =>
        new Dictionary<string, Action<IParseNode>>
        {
            { "order", n => { Order = n.GetObjectValue<OrderV1>(OrderV1.CreateFromDiscriminatorValue); } },
            { "update", n => { Update = n.GetObjectValue<OrderUpdatedV1Update>(OrderUpdatedV1Update.CreateFromDiscriminatorValue); } },
        };

    /// <inheritdoc />
    public void Serialize(ISerializationWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteObjectValue("order", Order);
        writer.WriteObjectValue("update", Update);
        writer.WriteAdditionalData(AdditionalData);
    }
}

/// <summary>
/// Discriminated update descriptor for the <c>order.updated</c> webhook event.
/// The <see cref="Type"/> discriminator is one of: <c>STATUS</c>, <c>SHIPPING.ADDRESS</c>, <c>EMAIL</c>.
/// </summary>
public sealed class OrderUpdatedV1Update : IAdditionalDataHolder, IParsable
{
    /// <inheritdoc />
    public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

    /// <summary>Gets or sets the update type discriminator.</summary>
    public string? Type { get; set; }

    /// <summary>Creates a new instance from a parse node (discriminator factory).</summary>
    public static OrderUpdatedV1Update CreateFromDiscriminatorValue(IParseNode parseNode)
    {
        ArgumentNullException.ThrowIfNull(parseNode);
        return new OrderUpdatedV1Update();
    }

    /// <inheritdoc />
    public IDictionary<string, Action<IParseNode>> GetFieldDeserializers() =>
        new Dictionary<string, Action<IParseNode>>
        {
            { "type", n => { Type = n.GetStringValue(); } },
        };

    /// <inheritdoc />
    public void Serialize(ISerializationWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue("type", Type);
        writer.WriteAdditionalData(AdditionalData);
    }
}
