using System.Threading;
using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Impworks.Utils.Strings;
using Jering.Javascript.NodeJS;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    public abstract class TreeLayoutJobBase: JobBase
    {
        protected TreeLayoutJobBase(AppDbContext db, INodeJSService js, BonsaiConfigService config, ILogger logger)
        {
            _db = db;
            _js = js;
            _config = config;
            _logger = logger;
        }

        protected readonly AppDbContext _db;
        protected readonly INodeJSService _js;
        protected readonly BonsaiConfigService _config;
        protected readonly ILogger _logger;

        /// <summary>
        /// Creates a relation context with people only.
        /// </summary>
        protected async Task<RelationContext> GetRelationContextAsync()
        {
            var opts = new RelationContextOptions { PeopleOnly = true };
            return await RelationContext.LoadContextAsync(_db, opts);
        }

        /// <summary>
        /// Renders the tree using ELK.js.
        /// </summary>
        protected async Task<string> RenderTreeAsync(TreeLayoutVM tree, int thoroughness, CancellationToken token)
        {
            var json = JsonConvert.SerializeObject(tree);
            var result = await _js.InvokeFromFileAsync<string>(
                "./External/tree/tree-layout.js",
                args: [json, thoroughness],
                cancellationToken: token
            );

            if (string.IsNullOrEmpty(result))
                throw new Exception("Failed to render tree: output is empty.");

            return result;
        }

        /// <summary>
        /// Returns the photo for a card, depending on the gender, reverting to a default one if unspecified.
        /// </summary>
        protected string GetPhoto(string actual, bool gender)
        {
            var defaultPhoto = gender
                ? "~/assets/img/unknown-male.png"
                : "~/assets/img/unknown-female.png";

            return StringHelper.Coalesce(
                MediaPresenterService.GetSizedMediaPath(actual, MediaSize.Small),
                defaultPhoto
            );
        }
    }
}
