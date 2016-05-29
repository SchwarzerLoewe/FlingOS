﻿#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Framework.Processes.Requests.Devices;
using Kernel.Devices;
using Kernel.Framework;

namespace Kernel.FileSystems
{
    /// <summary>
    ///     Represents a partition on a disk drive.
    /// </summary>
    public class Partition : DiskDevice
    {
        public bool Mapped = false;

        /// <summary>
        ///     The sector number at which the partition starts.
        /// </summary>
        internal ulong StartingSector;

        /// <summary>
        ///     The underlying disk device on which this partition resides.
        /// </summary>
        public DiskDevice TheDiskDevice;

        /// <summary>
        ///     The ID of this partition (volume).
        /// </summary>
        public String VolumeID = "[NO ID]";

        /// <summary>
        ///     Initializes a new partition.
        /// </summary>
        /// <param name="aDiskDevice">The disk device on which the partition resides.</param>
        /// <param name="aStartingSector">The sector number at which the partition starts.</param>
        /// <param name="aSectorCount">The number of sectors in the partition.</param>
        public Partition(DiskDevice aDiskDevice, ulong aStartingSector, ulong aSectorCount)
            : base(DeviceGroup.Storage, DeviceClass.Storage, DeviceSubClass.Virtual, "Partition", new uint[0], true)
        {
            TheDiskDevice = aDiskDevice;
            StartingSector = aStartingSector;
        }

        public override ulong BlockCount
        {
            get { return TheDiskDevice.BlockCount; }
        }

        public override ulong BlockSize
        {
            get { return TheDiskDevice.BlockSize; }
        }

        /// <summary>
        ///     Reads contiguous blocks within the partition. Block 0 = 1st sector of the partition.
        /// </summary>
        /// <param name="aBlockNo">The first sector (block) number to read.</param>
        /// <param name="aBlockCount">The number of sectors (blocks) to read.</param>
        /// <param name="aData">The buffer to read into.</param>
        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ulong DiskBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.ReadBlock(DiskBlockNo, aBlockCount, aData);
        }

        /// <summary>
        ///     See base class.
        /// </summary>
        /// <param name="aBlockNo">See base class.</param>
        /// <param name="aBlockCount">See base class.</param>
        /// <param name="aData">See base class.</param>
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
            ulong xHostBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.WriteBlock(xHostBlockNo, aBlockCount, aData);
        }

        /// <summary>
        ///     Determines whether the specified disk has had any valid partitions detected.
        /// </summary>
        /// <param name="disk">The disk to check.</param>
        /// <returns>Whether the specified disk has had any valid partitions detected.</returns>
        public static bool HasPartitions(DiskDevice disk)
        {
            for (int i = 0; i < PartitionManager.Partitions.Count; i++)
            {
                Partition part = (Partition) PartitionManager.Partitions[i];
                if (part.TheDiskDevice == disk)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Gets the first partition, if any, of the specified disk.
        /// </summary>
        /// <param name="disk">The disk to get the first partition of.</param>
        /// <returns>The partition or null if none found.</returns>
        public static Partition GetFirstPartition(DiskDevice disk)
        {
            for (int i = 0; i < PartitionManager.Partitions.Count; i++)
            {
                Partition part = (Partition) PartitionManager.Partitions[i];
                if (part.TheDiskDevice == disk)
                {
                    return part;
                }
            }
            return null;
        }

        /// <summary>
        ///     Cleans any caches of data which haven't been committed to disk.
        /// </summary>
        public override void CleanCaches()
        {
            //Pass it down the chain
            TheDiskDevice.CleanCaches();
        }
    }
}