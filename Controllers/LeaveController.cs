using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;

namespace TimeMate.Controllers
{
    public class LeaveController : Controller
    {
        private readonly TimeMateContext _context;

        public LeaveController(TimeMateContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var timeMateContext = _context.leaveRequests.Include(l => l.employee);
            return View(await timeMateContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null || _context.leaveRequests == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.leaveRequests
                .Include(l => l.employee)
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        // GET: Leave/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.TimeMateUsers, "Id", "Id");
            return View();
        }

        // POST: Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,startDate,endDate,Reason,status,CompensatoryDate,CancellationReason")] LeaveRequest leaveRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(leaveRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.TimeMateUsers, "Id", "Id", leaveRequest.EmployeeId);
            return View(leaveRequest);
        }

        // GET: Leave/Edit/5
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null || _context.leaveRequests == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.leaveRequests.FindAsync(Id);
            if (leaveRequest == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.TimeMateUsers, "Id", "Id", leaveRequest.EmployeeId);
            return View(leaveRequest);
        }

        // POST: Leave/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, [Bind("Id,EmployeeId,startDate,endDate,Reason,status,CompensatoryDate,CancellationReason")] LeaveRequest leaveRequest)
        {
            if (Id != leaveRequest.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaveRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeaveRequestExists(leaveRequest.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.TimeMateUsers, "Id", "Id", leaveRequest.EmployeeId);
            return View(leaveRequest);
        }

        // GET: Leave/Delete/5
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null || _context.leaveRequests == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.leaveRequests
                .Include(l => l.employee)
                .FirstOrDefaultAsync(m => m.Id == Id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            return View(leaveRequest);
        }

        // POST: Leave/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int Id)
        {
            if (_context.leaveRequests == null)
            {
                return Problem("Entity set 'TimeMateContext.leaveRequests'  is null.");
            }
            var leaveRequest = await _context.leaveRequests.FindAsync(Id);
            if (leaveRequest != null)
            {
                _context.leaveRequests.Remove(leaveRequest);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeaveRequestExists(int Id)
        {
          return (_context.leaveRequests?.Any(e => e.Id == Id)).GetValueOrDefault();
        }
        // GET: Leave/Approve/5
        public async Task<IActionResult> Approve(int? Id)
        {
            if (Id == null || _context.leaveRequests == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.leaveRequests.FindAsync(Id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            leaveRequest.ManagerApproval = LeaveStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var timeMateContext = _context.leaveRequests.Include(l => l.employee);
            return View(await timeMateContext.ToListAsync());
        }
        // GET: Leave/Reject/5
        public async Task<IActionResult> Reject(int? Id)
        {
            if (Id == null || _context.leaveRequests == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.leaveRequests.FindAsync(Id);
            if (leaveRequest == null)
            {
                return NotFound();
            }

            leaveRequest.ManagerApproval = LeaveStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
