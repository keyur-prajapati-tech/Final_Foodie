using Foodie.Models.Restaurant;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Foodie.Controllers.Restaurant
{

    public class AddrestaurantController : Controller
    {
        private readonly IRestaurantRepository _repository;

        public AddrestaurantController(IRestaurantRepository repository)
        {
            _repository = repository;
        }


        public IActionResult Registerres()
        {

            int r_id = RestaurantRepository.getRId();


            if (r_id != 0)
            {
                RestaurantRegistrationViewModel model = new RestaurantRegistrationViewModel
                {

                    tbl_Restaurant = _repository.getRestaurant(r_id),
                    tbl_Owner = _repository.getOwner(r_id)
                };
                return View(model);

            }
            else
            {

                return View();
            }
        }


        [HttpPost]
        public IActionResult Registerres(RestaurantRegistrationViewModel model)
        {
            int r_id = RestaurantRepository.getRId();

            if (r_id == 0)
            {

                if (!ModelState.IsValid)
                {
                    var owner = new tbl_owner_details
                    {
                        owner_name = model.tbl_Owner.owner_name,
                        owner_email = model.tbl_Owner.owner_email,
                        owner_contact = model.tbl_Owner.owner_contact
                    };

                    int ownerid = _repository.AddOwner(owner);

                    var restaurant = new tbl_restaurant
                    {
                        restaurant_name = model.tbl_Restaurant.restaurant_name,
                        restaurant_contact = model.tbl_Owner.owner_contact,
                        restaurant_email = model.tbl_Owner.owner_email,
                        restaurant_street = model.tbl_Restaurant.restaurant_street,
                        restaurant_pincode = model.tbl_Restaurant.restaurant_pincode,
                        restaurant_lat = model.tbl_Restaurant.restaurant_lat,
                        restaurant_lag = model.tbl_Restaurant.restaurant_lag,
                        restaurant_isApprov = false,
                        restaurant_isOnline = false,
                        est_id = 1,
                        owner_id = model.tbl_Owner.owner_id
                    };

                    _repository.AddRestaurant(restaurant);


                    return RedirectToAction("MenuItems");

                }
            }
            else
            {
                return RedirectToAction("MenuItems");

            }
            return View();
        }

        //[Route("MenuItems")]
        public IActionResult MenuItems()
        {
            //int r_id = RestaurantRepository.getRId();

            //if (r_id != 0)
            //{
            //    var model = new RestaurantMenuViewmodel
            //    {
            //        tbl_Vendores_Img = _repository.getVendors_img(r_id),
            //        tbl_Vendor_Availability = _repository.getVendor_Available(r_id)
            //    };
            //    return View(model);
            //}
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> MenuItems(IFormFile r_img, IFormFile m_img, List<string> SelectedDays)
        {
            int r_id = RestaurantRepository.getRId();

            tbl_vendores_img tbl_Vendores_img = new tbl_vendores_img();

            tbl_Vendores_img.Restaurant_id = r_id;

            tbl_vendor_availability tbl_Vendor_Availability = new tbl_vendor_availability();

            tbl_Vendor_Availability.open_time = DateTime.Now.Date;
            tbl_Vendor_Availability.close_time = DateTime.Now.Date;
            tbl_Vendor_Availability.is_Open = true;
            tbl_Vendor_Availability.Restaurant_id = r_id;

            tbl_Vendor_Availability.day_of_week = SelectedDays != null ? String.Join(",", SelectedDays) : "";


            if (r_img != null && m_img != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    await r_img.CopyToAsync(ms);
                    byte[] r_img_data = ms.ToArray();
                    tbl_Vendores_img.Restaurant_img = r_img_data;

                    await m_img.CopyToAsync(ms);
                    byte[] m_img_data = ms.ToArray();
                    tbl_Vendores_img.Restaurant_menu_img = m_img_data;


                    _repository.AddVendors_img(tbl_Vendores_img.Restaurant_img, tbl_Vendores_img.Restaurant_menu_img);

                    _repository.AddVendor_Avalaiable(tbl_Vendor_Availability);


                    return RedirectToAction("ResDocument");

                }
            }
            return View();
        }



        //[Route("ResDocument")]
        public IActionResult ResDocument()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResDocument(RestuarantDocumnetViewModel doc ,IFormFile pan_img, IFormFile fssai_img)
        {

            byte[] panImg = null;
            byte[] fssaiImg = null;

            int rId = RestaurantRepository.getRId();

            if (pan_img != null && fssai_img != null)
            {
                using(MemoryStream ms = new MemoryStream())
                {
                    await pan_img.CopyToAsync(ms);
                    panImg = ms.ToArray();


                    await fssai_img.CopyToAsync(ms);
                    fssaiImg = ms.ToArray();
                }

                tbl_pan_details pan = new tbl_pan_details()
                {
                    pan_number = doc.PanDetails.pan_number,
                    pan_holder_name = doc.PanDetails.pan_holder_name,
                    is_Verify = true,
                    Restaurant_id = rId,
                    img = panImg

                };

                tbl_gst_details gst = new tbl_gst_details()
                {
                    gst_number = doc.GstDetails.gst_number,
                    is_Verify = true,
                    Restaurant_id = rId
                };

                tbl_fssai_Details fssai = new tbl_fssai_Details()
                {
                    fssai_certi = doc.FssaiDetails.fssai_certi,
                    is_Verify = true,
                    Restaurant_id = rId,
                    Ex_Date = doc.FssaiDetails.Ex_Date,
                    img = fssaiImg
                };

                tbl_bank_details bank = new tbl_bank_details()
                {
                    bank_name = doc.BankDetails.bank_name,
                    IFSC_code = doc.BankDetails.IFSC_code,
                    ACC_NO = doc.BankDetails.ACC_NO,
                    Restaurant_id = rId,
                    ACC_Type = "Curent"
                };

                _repository.AddPanDetails(pan, panImg);
                _repository.AddGSTDetails(gst);
                _repository.AddFssaiDetails(fssai, fssaiImg);
                _repository.AddBankDetails(bank);

                return RedirectToAction("OrderReady", "Restaurant");
            }
            return View(); 
        }



        //[Route("home")]
        public IActionResult home()
        {
            return View();
        }


       
    }
}
