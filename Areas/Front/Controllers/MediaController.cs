using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Front.Controllers
{
    /// <summary>
    /// The controller for displaying media information.
    /// </summary>
    [Area("Front")]
    [Route("m")]
    public class MediaController: Controller
    {
        /// <summary>
        /// Displays media and details.
        /// </summary>
        [Route("{key}")]
        public async Task<ActionResult> View(string key)
        {
            throw new NotImplementedException();
        }
    }
}
