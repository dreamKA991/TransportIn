using Bunit;
using TransportIn.Components.Pages;

namespace TransportIn.Tests.Persistence;

[Collection("PostgreSQL")]
public class MapPersistenceTests(PostgresDatabase database)
{
    [PostgresFact]
    [Trait("Suite", "Persistence")]
    [Trait("KnownGap", "true")]
    public async Task MapZone_RemainsVisibleInANewSession()
    {
        const string title = "QA Зона біля складу";
        using (var ui = database.OpenSession("qa_fixture_admin"))
        {
            ui.JSInterop.SetupModule("./js/mapMonitor.js");
            var form = ui.RenderComponent<MapMonitor>();
            form.Fill("Напр. Перекриття мосту", title);
            form.Find("input[min='100'][max='5000']").Change("850");
            form.SelectWithOption("warning").Change("warning");
            form.ClickButton("Поставити на карті");
            await form.Instance.OnMapClicked(50.497, 30.451);
            Assert.Contains(title, form.Markup);
            Assert.Contains("850 м", form.Markup);
        }

        using var reopened = database.OpenSession("qa_fixture_admin");
        reopened.JSInterop.SetupModule("./js/mapMonitor.js");
        var map = reopened.RenderComponent<MapMonitor>();
        Assert.Contains(title, map.Markup);
    }
}
