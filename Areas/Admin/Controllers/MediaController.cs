using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Code.Utils.Validation;
using Bonsai.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for managing media files.
    /// </summary>
    [Route("admin/media")]
    public class MediaController: AdminControllerBase
    {
        public MediaController(MediaManagerService media, AppDbContext db)
        {
            _media = media;
            _db = db;
        }

        private readonly MediaManagerService _media;
        private readonly AppDbContext _db;

        /// <summary>
        /// Displays the list of pages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index(MediaListRequestVM request)
        {
            var vm = await _media.GetMediaAsync(request);
            return View(vm);
        }

        /// <summary>
        /// Displays the uploader form.
        /// </summary>
        [HttpGet]
        [Route("upload")]
        public async Task<ActionResult> Upload()
        {
            return View();
        }

        /// <summary>
        /// Handles a single file upload.
        /// </summary>
        [HttpPost]
        [Route("upload")]
        public async Task<ActionResult> Upload(IFormFile file)
        {
            var vm = new MediaUploadRequestVM
            {
                Name = file.FileName,
                MimeType = file.ContentType,
                Data = file.OpenReadStream(),
            };
            var result = await _media.UploadAsync(vm, User);
            return Json(result);
        }

        /// <summary>
        /// Displays the update form for a media file.
        /// </summary>
        [HttpGet]
        [Route("update")]
        public async Task<ActionResult> Update(Guid id)
        {
            var vm = await _media.RequestUpdateAsync(id);
            return View(vm);
        }

        /// <summary>
        /// Updates the media data.
        /// </summary>
        [HttpPost]
        [Route("update")]
        public async Task<ActionResult> Update(MediaEditorVM vm)
        {
            if(!ModelState.IsValid)
                return View(vm);

            try
            {
                await _media.UpdateAsync(vm, User);
                await _db.SaveChangesAsync();

                return RedirectToSuccess("Медиа-файл обновлен");
            }
            catch (ValidationException ex)
            {
                SetModelState(ex);
                return View(vm);
            }
        }

        /// <summary>
        /// Removes the media file.
        /// </summary>
        public async Task<ActionResult> Remove(Guid id)
        {
            await _media.RemoveAsync(id, User);
            return RedirectToSuccess("Медиа-файл удален");
        }
    }
}
