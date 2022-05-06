using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Common.Helpers;
using MyLeasing.Common.Models;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;

namespace MyLeasing.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PropertiesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public PropertiesController(
            DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> PostProperty([FromBody] PropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _dataContext.Owners.FindAsync(request.OwnerId);
                if (owner == null)
                {
                    return BadRequest("Not valid owner.");
                }

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return BadRequest("Not valid property type.");
                }

                var property = new Property
                {
                    Address = request.Address,
                    HasParkingLot = request.HasParkingLot,
                    IsAvailable = request.IsAvailable,
                    Neighborhood = request.Neighborhood,
                    Owner = owner,
                    Price = request.Price,
                    PropertyType = propertyType,
                    Remarks = request.Remarks,
                    Rooms = request.Rooms,
                    SquareMeters = request.SquareMeters,
                    Stratum = request.Stratum
                };

                _dataContext.Properties.Add(property);
                await _dataContext.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest("Not valid owner." + ex.Message);
            }
        }

        //Cuando se hace un segundo post en api s debe de rutear el metodo.
        [HttpPost]
        [Route("AddImageToProperty")]
        public async Task<IActionResult> AddImageToProperty([FromBody] ImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var property = await _dataContext.Properties.FindAsync(request.PropertyId);
                if (property == null)
                {
                    return BadRequest("Not valid property.");
                }

                var imageUrl = string.Empty;
                if (request.ImageArray != null && request.ImageArray.Length > 0)
                {
                    var stream = new MemoryStream(request.ImageArray);
                    var guid = Guid.NewGuid().ToString();
                    var file = $"{guid}.jpg";
                    var folder = "wwwroot\\images\\Properties";
                    var fullPath = $"~/images/Properties/{file}";
                    var response = FilesHelper.UploadPhoto(stream, folder, file);

                    if (response)
                    {
                        imageUrl = fullPath;
                    }
                }

                var propertyImage = new PropertyImage
                {
                    ImageUrl = imageUrl,
                    Property = property
                };

                _dataContext.PropertyImages.Add(propertyImage);
                await _dataContext.SaveChangesAsync();
                return Ok(propertyImage);
            }
            catch (Exception ex)
            {
                return BadRequest("Not valid property." + ex.Message);
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProperty([FromRoute] int id, [FromBody] PropertyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != request.Id)
                {
                    return BadRequest();
                }

                var oldProperty = await _dataContext.Properties.FindAsync(request.Id);
                if (oldProperty == null)
                {
                    return BadRequest("Property doesn't exists.");
                }

                var propertyType = await _dataContext.PropertyTypes.FindAsync(request.PropertyTypeId);
                if (propertyType == null)
                {
                    return BadRequest("Not valid property type.");
                }

                oldProperty.Address = request.Address;
                oldProperty.HasParkingLot = request.HasParkingLot;
                oldProperty.IsAvailable = request.IsAvailable;
                oldProperty.Neighborhood = request.Neighborhood;
                oldProperty.Price = request.Price;
                oldProperty.PropertyType = propertyType;
                oldProperty.Remarks = request.Remarks;
                oldProperty.Rooms = request.Rooms;
                oldProperty.SquareMeters = request.SquareMeters;
                oldProperty.Stratum = request.Stratum;

                _dataContext.Properties.Update(oldProperty);
                await _dataContext.SaveChangesAsync();
                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest("Property doesn't exists." + ex.Message);
            }
        }

        [HttpPost]
        [Route("DeleteImageToProperty")]
        public async Task<IActionResult> DeleteImageToProperty([FromBody] ImageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var propertyImage = await _dataContext.PropertyImages.FindAsync(request.Id);
                if (propertyImage == null)
                {
                    return BadRequest("Property image doesn't exist.");
                }

                _dataContext.PropertyImages.Remove(propertyImage);
                await _dataContext.SaveChangesAsync();
                return Ok(propertyImage);
            }
            catch (Exception ex)
            {
                return BadRequest("Property image doesn't exist." + ex.Message);
            }
        }

        [HttpGet("GetLastPropertyByOwnerId/{id}")]
        public async Task<IActionResult> GetLastPropertyByOwnerId([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var owner = await _dataContext.Owners
                    .Include(o => o.Properties)
                    .ThenInclude(p => p.PropertyType)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (owner == null)
                {
                    return NotFound();
                }

                var property = owner.Properties.LastOrDefault();
                var response = new PropertyResponse
                {
                    Address = property.Address,
                    HasParkingLot = property.HasParkingLot,
                    Id = property.Id,
                    IsAvailable = property.IsAvailable,
                    Neighborhood = property.Neighborhood,
                    Price = property.Price,
                    PropertyType = property.PropertyType.Name,
                    Remarks = property.Remarks,
                    Rooms = property.Rooms,
                    SquareMeters = property.SquareMeters,
                    Stratum = property.Stratum
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Property doesn't exist." + ex.Message);
            }
        }

        //Métodos para Angular
        [HttpGet]
        [Route("GetPropertiesWeb")]
        public async Task<IActionResult> GetProperties()
        {
            try
            {
                var properties = await _dataContext.Properties
                    .Include(p => p.PropertyType)
                    .Include(p => p.PropertyImages)
                    .Include(p => p.Contracts)
                    .Include(p => p.Owner)
                    .ThenInclude(o => o.User).ToListAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Listado de las propiedades.",
                    Result = properties
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el listado de las propiedades." + ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetPropertiesWeb/{propertyId}")]
        public async Task<IActionResult> GetProperties(int propertyId)
        {
            try
            {
                var property = await _dataContext.Properties
                    .Include(o => o.Owner)
                    .ThenInclude(o => o.User)
                    .Include(o => o.Contracts)
                    .ThenInclude(c => c.Lessee)
                    .ThenInclude(l => l.User)
                    .Include(o => o.PropertyType)
                    .Include(p => p.PropertyImages)
                    .FirstOrDefaultAsync(m => m.Id == propertyId);

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Datos de la propiedad.",
                    Result = property
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Se ha producido un error al intentar obtener el la información de la propiedad." + ex.Message
                });
            }
        }
    }
}

