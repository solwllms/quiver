#region

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Input;
using RGB.NET.Core;
using RGB.NET.Devices.CoolerMaster;
using RGB.NET.Devices.Corsair;
using RGB.NET.Devices.Razer;

#endregion

//using RGB.NET.Devices.Logitech;

namespace Quiver.system
{
    public class rgbDevice
    {
        private static RGBSurface _surface;
        private static Dictionary<LedId, Color> _rules;

        public static cvar cvarRgb = new cvar("rgb_enable", "1", true, true, callback: delegate
        {
            if (cvarRgb.Valueb()) Init();
            else Shutdown();
        });

        public static bool hasChanged;

        public static void Init()
        {
            if (!cvarRgb.Valueb()) return;

            log.WriteLine("initialising RGB api..");
            _surface = RGBSurface.Instance;
            _surface.Exception += args => Console.WriteLine(args.Exception.Message);

            _surface.LoadDevices(CorsairDeviceProvider.Instance);
            //surface.LoadDevices(LogitechDeviceProvider.Instance);
            //surface.LoadDevices(MsiDeviceProvider.Instance);
            _surface.LoadDevices(CoolerMasterDeviceProvider.Instance);
            _surface.LoadDevices(RazerDeviceProvider.Instance);

            TimerUpdateTrigger TimerTrigger = new TimerUpdateTrigger
            {
                UpdateFrequency = 0.05
            };
            RGBSurface.Instance.RegisterUpdateTrigger(TimerTrigger);
            TimerTrigger.Start();

            _surface.AlignDevices();

            foreach (var v in RGBSurface.Instance.Devices)
                log.WriteLine("RGB device connected " + v.DeviceInfo.Manufacturer + " " + v.DeviceInfo.Model + " (" +
                              v.DeviceInfo.DeviceType + ")");
            log.WriteLine("RGB Ready! ("+ RGBSurface.Instance.Devices.Count() + " devices loaded)", log.LogMessageType.Good);

            _rules = new Dictionary<LedId, Color>();
            _surface.Updating += args => Update();
        }

        public static void Shutdown()
        {
            _rules = null;
        }

        public static void Update()
        {
            if (_rules == null) return;
            foreach (var v in _surface.Leds)
                if (_rules.ContainsKey(v.Id))
                    v.Color = _rules[v.Id];
        }

        public static void SetBind(string bind, byte r, byte g, byte b)
        {
            if (_rules == null) return;

            var k = cmd.Getbind(bind);
            if (k == Key.Unknown) return;

            if (k == Key.Up)
            {
                Set(LedId.Keyboard_ArrowUp, r, g, b);
                return;
            }

            if (k == Key.Down)
            {
                Set(LedId.Keyboard_ArrowDown, r, g, b);
                return;
            }

            if (k == Key.Left)
            {
                Set(LedId.Keyboard_ArrowLeft, r, g, b);
                return;
            }

            if (k == Key.Right)
            {
                Set(LedId.Keyboard_ArrowRight, r, g, b);
                return;
            }

            var id = "Keyboard_" + k;
            LedId led;
            if (Enum.TryParse(id, out led)) Set(led, r, g, b);

            hasChanged = true;
        }

        public static void SetStr(string id, byte r, byte g, byte b)
        {
            if (_rules == null) return;
            _rules[(LedId) Enum.Parse(typeof(LedId), id)] = new Color(r, g, b);

            hasChanged = true;
        }

        public static void Set(LedId id, byte r, byte g, byte b)
        {
            if (_rules == null) return;
            _rules[id] = new Color(r, g, b);

            hasChanged = true;
        }

        public static void SetAll(byte r, byte g, byte b)
        {
            if (_rules == null) return;

            foreach (var v in _surface.Leds)
                _rules[v.Id] = new Color(r, g, b);

            hasChanged = true;
        }
    }
}