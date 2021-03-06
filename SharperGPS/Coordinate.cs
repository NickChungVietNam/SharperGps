/* Copyright 2007 - Morten Nielsen
 * Copyright (C) 2009  John Schmitt, Mike McBride, and Kevin Curtis
 * 
 * This file is part of SharperGps.
 * SharperGps is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharperGps is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with SharperGps; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * Change log
 * 
 * Date    | Author | Reason for change                | Description of change
 * -------- -------- ---------------------------------- ------------------------------------------------------
 * 12/9/09  jschmitt initial development                Fully implemented every method
 */
namespace Ares.SharperGps
{
    using System.Text;
    using System;

    /// <summary>
    /// Represents a coordinate in spherical space
    /// </summary>
    public class Coordinate
    {
        /// <summary>
        /// Initializes a coordinate object
        /// </summary>
        public Coordinate()
            : this(0, 0)
        {
        }

        /// <summary>
        /// Initializes a coordinate object
        /// </summary>
        public Coordinate(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Distance in meters, using the Harversine Formula (Great circle).
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Distance in meters</returns>
        /// <remarks>
        /// See:
        /// http://en.wikipedia.org/wiki/Great-circle_distance
        /// http://williams.best.vwh.net/avform.htm (Aviation Formulary)
        /// </remarks>
        public double Distance(Coordinate other)
        {
            const double rad = Math.PI / 180;
            const double nauticalMile = 1852;

            double lon1 = rad * -Longitude;
            double lat1 = rad * Latitude;
            double lon2 = rad * -other.Longitude;
            double lat2 = rad * other.Latitude;

            double d = 2 * Math.Asin(Math.Sqrt(
                                         Math.Pow(Math.Sin((lat1 - lat2) / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon1 - lon2) / 2), 2)
                                         ));
            return nauticalMile * 60 * d / rad;
        }

		
        internal void GetDM(out int latDeg, out double latMin, out bool north, out int lonDeg, out double lonMin, out bool east)
        {
            north = Latitude >= 0;
            double a = Math.Abs(Latitude);
            latDeg = (int)Math.Floor(a);
            latMin = (a - latDeg) * 60;

            east = Longitude >= 0;
            double b = Math.Abs(Longitude);
            lonDeg = (int)Math.Floor(b);
            lonMin = (b - lonDeg) * 60;
        }
        internal void GetDMS(out int latDeg, out int latMin, out double latSec, out bool north, out int lonDeg, out int lonMin, out double lonSec, out bool east)
        {
            north = Latitude >= 0;
            double a = Math.Abs(Latitude);
            latDeg = (int)Math.Floor(a);
            a -= latDeg;
            latMin = (int)Math.Floor(a * 60.0);
            latSec = (a * 60.0 - latMin) * 60.0;

            east = Longitude >= 0;
            double b = Math.Abs(Longitude);
            lonDeg = (int)Math.Floor(b);
            b -= lonDeg;
            lonMin = (int)Math.Floor(b * 60.0);
            lonSec = (b * 60.0 - lonMin) * 60.0;
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(null);
        }
        /// <summary>
        /// Valid formats: 
        /// String.Empty or "D": Decimal degrees
        /// DM: Degrees, minutes
        /// DMS: Degrees, minutes seconds
        /// </summary>
        /// <param name="format">Formatting string</param>
        /// <returns></returns>
        public string ToString(string format)
        {
            return ToString(format, null);
        }
        /// <summary>
        /// Valid formats: 
        /// String.Empty or "D": Decimal degrees
        /// DM: Degrees, minutes
        /// DMS: Degrees, minutes seconds
        /// </summary>
        /// <param name="format">Formatting string</param>
        /// <param name="formatProvider">IFormatProvider</param>
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();

            if (String.IsNullOrEmpty(format))
                format = "D";

            switch (format.ToUpper())
            {
                case "D":
                    sb.AppendFormat(formatProvider, "{0:0#.0000}�{1} ", Math.Abs(Latitude), (Latitude > 0 ? "N" : "S"));
                    sb.AppendFormat(formatProvider, "{0:0##.0000}�{1}", Math.Abs(Longitude), (Longitude > 0 ? "E" : "W"));
                    break;
                case "DM":
                    {
                        int latDeg, lonDeg;
                        double latMin, lonMin;
                        bool north, east;
                        GetDM(out latDeg, out latMin, out north, out lonDeg, out lonMin, out east);
                        sb.AppendFormat(formatProvider, "{0:0#}�{1:0#.000}'{2} ", latDeg, latMin, (north ? "N" : "S"));
                        sb.AppendFormat(formatProvider, "{0:0##}�{1:0#.000}'{2}", lonDeg, lonMin, (east ? "E" : "W"));
                    }
                    break;
                case "DMS":
                    {
                        int latDeg, latMin, lonDeg, lonMin;
                        double latSec, lonSec;
                        bool north, east;
                        GetDMS(out latDeg, out latMin, out latSec, out north, out lonDeg, out lonMin, out lonSec, out east);
                        sb.AppendFormat(formatProvider, "{0:0#}�{1:0#}'{2:0#.0}\"{3} ", latDeg, latMin, latSec, (north ? "N" : "S"));
                        sb.AppendFormat(formatProvider, "{0:0##}�{1:0#}'{2:0#.0}\"{3}", lonDeg, lonMin, lonSec, (east ? "E" : "W"));
                        break;
                    }
                default:
                    throw new ArgumentException("Coordinate.ToString(): Invalid formatting string.");
            }
            return sb.ToString();
        }
    }
}