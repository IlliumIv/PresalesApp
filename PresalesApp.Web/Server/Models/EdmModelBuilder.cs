using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using PresalesApp.DistanceCalculator;

namespace PresalesApp.Web.Server.Models;

public class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataModelBuilder();

        var moduleMessage = builder.EntityType<Module>();
        moduleMessage.HasKey(x => x.Id);
        moduleMessage.Property(x => x.Name);
        moduleMessage.Property(x => x.WidthMinPercentage);
        moduleMessage.Property(x => x.HeightMinPercentage);
        moduleMessage.Property(x => x.PixelsMin);
        moduleMessage.Property(x => x.WidthMinInMeters);
        moduleMessage.Property(x => x.HeightMinInMeters);
        moduleMessage.Property(x => x.Description);

        builder.EntitySet<Module>("Modules");

        return builder.GetEdmModel();
    }
}
