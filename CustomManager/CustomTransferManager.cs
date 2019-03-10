﻿using ColossalFramework;
using RealConstruction.CustomAI;
using RealConstruction.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RealConstruction.CustomManager
{
    public class CustomTransferManager: TransferManager
    {
        public static void StartSpecialBuildingTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
        {
                VehicleInfo vehicleInfo = null;
                if (material == (TransferManager.TransferReason)110)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialForestry, ItemClass.Level.Level1);
                }
                else if (material == (TransferManager.TransferReason)111)
                {
                    vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialFarming, ItemClass.Level.Level1);
                }


            if (vehicleInfo != null)
            {
                Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
                ushort num16;
                if (Singleton<VehicleManager>.instance.CreateVehicle(out num16, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, data.m_position, material, false, true))
                {
                    vehicleInfo.m_vehicleAI.SetSource(num16, ref vehicles.m_buffer[(int)num16], buildingID);
                    if (vehicleInfo.m_vehicleAI is CargoTruckAI)
                    {
                        CargoTruckAI AI = vehicleInfo.m_vehicleAI as CargoTruckAI;
                        CustomCargoTruckAI.CargoTruckAISetSourceForRealConstruction(num16, ref vehicles.m_buffer[(int)num16], buildingID);
                        vehicles.m_buffer[(int)num16].m_transferSize = (ushort)AI.m_cargoCapacity;
                    }
                    else
                    {
                        DebugLog.LogToFileOnly("Error: vehicleInfo is not cargoTruckAI " + vehicleInfo.m_vehicleAI.ToString());
                    }
                    vehicleInfo.m_vehicleAI.StartTransfer(num16, ref vehicles.m_buffer[(int)num16], material, offer);
                    ushort building4 = offer.Building;
                    if (building4 != 0)
                    {
                        int amount;
                        int num17;
                        vehicleInfo.m_vehicleAI.GetSize(num16, ref vehicles.m_buffer[(int)num16], out amount, out num17);
                    }
                }
            }
        }

        public static void StartTransfer(TransferManager.TransferReason material, TransferManager.TransferOffer offerOut, TransferManager.TransferOffer offerIn, int delta)
        {
            bool active = offerIn.Active;
            bool active2 = offerOut.Active;
            if (active && offerIn.Vehicle != 0)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Vehicle");
            }
            else if (active2 && offerOut.Vehicle != 0)
            {
                DebugLog.LogToFileOnly("Error: active2 && offerOut.Vehicle");
            }
            else if (active && offerIn.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Citizen");
            }
            else if (active2 && offerOut.Citizen != 0u)
            {
                DebugLog.LogToFileOnly("Error: active2 && offerOut.Citizen");
            }
            else if (active2 && offerOut.Building != 0)
            {
                Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
                ushort building = offerOut.Building;
                BuildingInfo info3 = buildings.m_buffer[(int)building].Info;
                offerIn.Amount = delta;
                if (RealConstructionThreading.IsSpecialBuilding(building))
                {
                    StartSpecialBuildingTransfer(building, ref buildings.m_buffer[(int)building], material, offerIn);
                }
                else
                {
                    DebugLog.LogToFileOnly("Error: active2 && offerOut.Building");
                }
            }
            else if (active && offerIn.Building != 0)
            {
                DebugLog.LogToFileOnly("Error: active && offerIn.Building");
            }
        }

        public static void Init()
        {
            DebugLog.LogToFileOnly("Init fake transfer manager");
            try
            {
                var inst = Singleton<TransferManager>.instance;
                var incomingCount = typeof(TransferManager).GetField("m_incomingCount", BindingFlags.NonPublic | BindingFlags.Instance);
                var incomingOffers = typeof(TransferManager).GetField("m_incomingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
                var incomingAmount = typeof(TransferManager).GetField("m_incomingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingCount = typeof(TransferManager).GetField("m_outgoingCount", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingOffers = typeof(TransferManager).GetField("m_outgoingOffers", BindingFlags.NonPublic | BindingFlags.Instance);
                var outgoingAmount = typeof(TransferManager).GetField("m_outgoingAmount", BindingFlags.NonPublic | BindingFlags.Instance);
                if (inst == null)
                {
                    DebugLog.LogToFileOnly("No instance of TransferManager found!");
                    return;
                }
                _incomingCount = incomingCount.GetValue(inst) as ushort[];
                _incomingOffers = incomingOffers.GetValue(inst) as TransferManager.TransferOffer[];
                _incomingAmount = incomingAmount.GetValue(inst) as int[];
                _outgoingCount = outgoingCount.GetValue(inst) as ushort[];
                _outgoingOffers = outgoingOffers.GetValue(inst) as TransferManager.TransferOffer[];
                _outgoingAmount = outgoingAmount.GetValue(inst) as int[];
                if (_incomingCount == null || _incomingOffers == null || _incomingAmount == null || _outgoingCount == null || _outgoingOffers == null || _outgoingAmount == null)
                {
                    DebugLog.LogToFileOnly("Arrays are null");
                }
            }
            catch (Exception ex)
            {
                DebugLog.LogToFileOnly("Exception: " + ex.Message);
            }
        }

        private static TransferManager.TransferOffer[] _incomingOffers;
        private static ushort[] _incomingCount;
        private static int[] _incomingAmount;
        private static TransferManager.TransferOffer[] _outgoingOffers;
        private static ushort[] _outgoingCount;
        private static int[] _outgoingAmount;
        private static bool _init;


        public static void CustomSimulationStepImpl()
        {
            if (!_init)
            {
                _init = true;
                Init();
            }

            int frameIndex = (int)(Singleton<SimulationManager>.instance.m_currentFrameIndex & 255u);
            if (frameIndex == 205)
            {
                //construction resource matchoffer
                MatchOffers((TransferReason)110);
            } 
            else if (frameIndex == 213)
            {
                //operation resource matchoffer
                MatchOffers((TransferReason)111);
            }
        }

        private static bool CanUseNewMatchOffers(ushort buildingID)
        {
            //For RealConstruction Mod, always use new matchoffers
            return true;
        }

        private static byte MatchOffersMode(TransferReason material)
        {
            //For RealConstruction Mod, always use incoming first mode
            //incoming first mode 0
            //outgoing first mode 1
            //balanced mode 2
            return 0;
        }


        private static void MatchOffers(TransferReason material)
        {
            if (material != TransferReason.None)
            {
                float distanceMultiplier = 1E-07f;
                float maxDistance = (distanceMultiplier == 0f) ? 0f : (0.01f / distanceMultiplier);
                for (int priority = 7; priority >= 0; priority--)
                {
                    int offerIdex = (int)material * 8 + priority;
                    int incomingCount = _incomingCount[offerIdex];
                    int outgoingCount = _outgoingCount[offerIdex];
                    int incomingIdex = 0;
                    int outgoingIdex = 0;
                    int oldPriority = priority;
                    // NON-STOCK CODE START
                    byte matchOffersMode = MatchOffersMode(material);
                    bool isLoopValid = false;
                    if (matchOffersMode == 2)
                    {
                        isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                    }
                    else
                    {
                        isLoopValid = (incomingIdex < incomingCount && outgoingIdex < outgoingCount);
                    }
                    // NON-STOCK CODE END
                    while (isLoopValid)
                    {
                        //use incomingOffer to match outgoingOffer
                        if (incomingIdex < incomingCount && (matchOffersMode != 1))
                        {
                            TransferOffer incomingOffer = _incomingOffers[offerIdex * 256 + incomingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(incomingOffer.Building);
                            Vector3 incomingPositionNew = Vector3.zero;
                            if (canUseNewMatchOffers)
                            {
                                incomingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOffer.Building].m_position;
                            }
                            // NON-STOCK CODE END
                            Vector3 incomingPosition = incomingOffer.Position;
                            int incomingOfferAmount = incomingOffer.Amount;
                            do
                            {
                                int incomingPriority = Mathf.Max(0, 2 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                if (canUseNewMatchOffers)
                                {
                                    priority = 7;
                                    incomingPriority = 0;
                                }
                                else
                                {
                                    priority = oldPriority;
                                    incomingPriority = Mathf.Max(0, 2 - priority);
                                }
                                // NON-STOCK CODE END
                                int incomingPriorityExclude = (!incomingOffer.Exclude) ? incomingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validOutgoingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int outgoingIdexInsideIncoming = outgoingIdex;
                                for (int incomingPriorityInside = priority; incomingPriorityInside >= incomingPriority; incomingPriorityInside--)
                                {
                                    int outgoingIdexWithPriority = (int)material * 8 + incomingPriorityInside;
                                    int outgoingCountWithPriority = _outgoingCount[outgoingIdexWithPriority];
                                    //To let incomingPriorityInsideFloat!=0
                                    float incomingPriorityInsideFloat = (float)incomingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    //UseNewMatchOffers to find the shortest transfer building
                                    if ((distanceOffsetPre >= incomingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    //Find the nearest offer to match in every priority.
                                    for (int i = outgoingIdexInsideIncoming; i < outgoingCountWithPriority; i++)
                                    {
                                        TransferOffer outgoingOfferPre = _outgoingOffers[outgoingIdexWithPriority * 256 + i];
                                        if (incomingOffer.m_object != outgoingOfferPre.m_object && (!outgoingOfferPre.Exclude || incomingPriorityInside >= incomingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(outgoingOfferPre.Position - incomingPosition);
                                            // NON-STOCK CODE START
                                            Vector3 outgoingPositionNew = Vector3.zero;
                                            float incomingOutgoingDistanceNew = 0;
                                            if (canUseNewMatchOffers)
                                            {
                                                outgoingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOfferPre.Building].m_position;
                                                incomingOutgoingDistanceNew = Vector3.SqrMagnitude(outgoingPositionNew - incomingPositionNew);
                                                if ((incomingOutgoingDistanceNew < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    validPriority = incomingPriorityInside;
                                                    validOutgoingIdex = i;
                                                    currentShortestDistance = incomingOutgoingDistanceNew;
                                                }
                                            }
                                            // NON-STOCK CODE END
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (incomingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (incomingPriorityInsideFloat - incomingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
                                            {
                                                validPriority = incomingPriorityInside;
                                                validOutgoingIdex = i;
                                                distanceOffsetPre = distanceOffset;
                                                if ((incomingOutgoingDistance < maxDistance))
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    outgoingIdexInsideIncoming = 0;
                                }
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get outgoingOffer
                                int matchedOutgoingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer outgoingOffer = _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex];
                                int outgoingOfferAmount = outgoingOffer.Amount;
                                int matchedOfferAmount = Mathf.Min(incomingOfferAmount, outgoingOfferAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    StartTransfer(material, outgoingOffer, incomingOffer, matchedOfferAmount);
                                }
                                incomingOfferAmount -= matchedOfferAmount;
                                outgoingOfferAmount -= matchedOfferAmount;
                                //matched outgoingOffer is empty now
                                if (outgoingOfferAmount == 0)
                                {
                                    int outgoingCountPost = _outgoingCount[matchedOutgoingOfferIdex] - 1;
                                    _outgoingCount[matchedOutgoingOfferIdex] = (ushort)outgoingCountPost;
                                    _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = _outgoingOffers[matchedOutgoingOfferIdex * 256 + outgoingCountPost];
                                    if (matchedOutgoingOfferIdex == offerIdex)
                                    {
                                        outgoingCount = outgoingCountPost;
                                    }
                                }
                                else
                                {
                                    outgoingOffer.Amount = outgoingOfferAmount;
                                    _outgoingOffers[matchedOutgoingOfferIdex * 256 + validOutgoingIdex] = outgoingOffer;
                                }
                                incomingOffer.Amount = incomingOfferAmount;
                            }
                            while (incomingOfferAmount != 0);
                            //matched incomingOffer is empty now
                            if (incomingOfferAmount == 0)
                            {
                                incomingCount--;
                                _incomingCount[offerIdex] = (ushort)incomingCount;
                                _incomingOffers[offerIdex * 256 + incomingIdex] = _incomingOffers[offerIdex * 256 + incomingCount];
                            }
                            else
                            {
                                incomingOffer.Amount = incomingOfferAmount;
                                _incomingOffers[offerIdex * 256 + incomingIdex] = incomingOffer;
                                incomingIdex++;
                            }
                        }
                        //For RealConstruction, We only satisify incoming building
                        //use outgoingOffer to match incomingOffer
                        if (outgoingIdex < outgoingCount && (matchOffersMode != 0))
                        {
                            TransferOffer outgoingOffer = _outgoingOffers[offerIdex * 256 + outgoingIdex];
                            // NON-STOCK CODE START
                            bool canUseNewMatchOffers = CanUseNewMatchOffers(outgoingOffer.Building);
                            Vector3 outgoingPositionNew = Vector3.zero;
                            if (canUseNewMatchOffers)
                            {
                                outgoingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[outgoingOffer.Building].m_position;
                            }
                            // NON-STOCK CODE END
                            Vector3 outgoingPosition = outgoingOffer.Position;
                            int outgoingOfferAmount = outgoingOffer.Amount;
                            do
                            {
                                int outgoingPriority = Mathf.Max(0, 2 - priority);
                                // NON-STOCK CODE START
                                float currentShortestDistance = -1f;
                                if (canUseNewMatchOffers)
                                {
                                    priority = 7;
                                    outgoingPriority = 0;
                                }
                                else
                                {
                                    priority = oldPriority;
                                    outgoingPriority = Mathf.Max(0, 2 - priority);
                                }
                                // NON-STOCK CODE END
                                int outgoingPriorityExclude = (!outgoingOffer.Exclude) ? outgoingPriority : Mathf.Max(0, 3 - priority);
                                int validPriority = -1;
                                int validIncomingIdex = -1;
                                float distanceOffsetPre = -1f;
                                int incomingIdexInsideOutgoing = incomingIdex;
                                for (int outgoingPriorityInside = priority; outgoingPriorityInside >= outgoingPriority; outgoingPriorityInside--)
                                {
                                    int incomingIdexWithPriority = (int)material * 8 + outgoingPriorityInside;
                                    int incomingCountWithPriority = _incomingCount[incomingIdexWithPriority];
                                    //To let outgoingPriorityInsideFloat!=0
                                    float outgoingPriorityInsideFloat = (float)outgoingPriorityInside + 0.1f;
                                    //Higher priority will get more chance to match
                                    if ((distanceOffsetPre >= outgoingPriorityInsideFloat) && !canUseNewMatchOffers)
                                    {
                                        break;
                                    }
                                    for (int j = incomingIdexInsideOutgoing; j < incomingCountWithPriority; j++)
                                    {
                                        TransferOffer incomingOfferPre = _incomingOffers[incomingIdexWithPriority * 256 + j];
                                        if (outgoingOffer.m_object != incomingOfferPre.m_object && (!incomingOfferPre.Exclude || outgoingPriorityInside >= outgoingPriorityExclude))
                                        {
                                            float incomingOutgoingDistance = Vector3.SqrMagnitude(incomingOfferPre.Position - outgoingPosition);
                                            // NON-STOCK CODE START
                                            Vector3 incomingPositionNew = Vector3.zero;
                                            float incomingOutgoingDistanceNew = 0;
                                            if (canUseNewMatchOffers)
                                            {
                                                incomingPositionNew = Singleton<BuildingManager>.instance.m_buildings.m_buffer[incomingOfferPre.Building].m_position;
                                                incomingOutgoingDistanceNew = Vector3.SqrMagnitude(outgoingPositionNew - incomingPositionNew);
                                                if ((incomingOutgoingDistanceNew < currentShortestDistance) || currentShortestDistance == -1)
                                                {
                                                    validPriority = outgoingPriorityInside;
                                                    validIncomingIdex = j;
                                                    currentShortestDistance = incomingOutgoingDistanceNew;
                                                }
                                            }
                                            // NON-STOCK CODE END
                                            float distanceOffset = (!(distanceMultiplier < 0f)) ? (outgoingPriorityInsideFloat / (1f + incomingOutgoingDistance * distanceMultiplier)) : (outgoingPriorityInsideFloat - outgoingPriorityInsideFloat / (1f - incomingOutgoingDistance * distanceMultiplier));
                                            if ((distanceOffset > distanceOffsetPre) && !canUseNewMatchOffers)
                                            {
                                                validPriority = outgoingPriorityInside;
                                                validIncomingIdex = j;
                                                distanceOffsetPre = distanceOffset;
                                                if (incomingOutgoingDistance < maxDistance)
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    incomingIdexInsideOutgoing = 0;
                                }
                                if (validPriority == -1)
                                {
                                    break;
                                }
                                //Find a validPriority, get incomingOffer
                                int matchedIncomingOfferIdex = (int)material * 8 + validPriority;
                                TransferOffer incomingOffers = _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex];
                                int incomingOffersAmount = incomingOffers.Amount;
                                int matchedOfferAmount = Mathf.Min(outgoingOfferAmount, incomingOffersAmount);
                                if (matchedOfferAmount != 0)
                                {
                                    StartTransfer(material, outgoingOffer, incomingOffers, matchedOfferAmount);
                                }
                                outgoingOfferAmount -= matchedOfferAmount;
                                incomingOffersAmount -= matchedOfferAmount;
                                //matched incomingOffer is empty now
                                if (incomingOffersAmount == 0)
                                {
                                    int incomingCountPost = _incomingCount[matchedIncomingOfferIdex] - 1;
                                    _incomingCount[matchedIncomingOfferIdex] = (ushort)incomingCountPost;
                                    _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = _incomingOffers[matchedIncomingOfferIdex * 256 + incomingCountPost];
                                    if (matchedIncomingOfferIdex == offerIdex)
                                    {
                                        incomingCount = incomingCountPost;
                                    }
                                }
                                else
                                {
                                    incomingOffers.Amount = incomingOffersAmount;
                                    _incomingOffers[matchedIncomingOfferIdex * 256 + validIncomingIdex] = incomingOffers;
                                }
                                outgoingOffer.Amount = outgoingOfferAmount;
                            }
                            while (outgoingOfferAmount != 0);
                            //matched outgoingOffer is empty now
                            if (outgoingOfferAmount == 0)
                            {
                                outgoingCount--;
                                _outgoingCount[offerIdex] = (ushort)outgoingCount;
                                _outgoingOffers[offerIdex * 256 + outgoingIdex] = _outgoingOffers[offerIdex * 256 + outgoingCount];
                            }
                            else
                            {
                                outgoingOffer.Amount = outgoingOfferAmount;
                                _outgoingOffers[offerIdex * 256 + outgoingIdex] = outgoingOffer;
                                outgoingIdex++;
                            }
                        }
                        
                        if (matchOffersMode == 2)
                        {
                            isLoopValid = (incomingIdex < incomingCount || outgoingIdex < outgoingCount);
                        }
                        else
                        {
                            isLoopValid = (incomingIdex < incomingCount && outgoingIdex < outgoingCount);
                        }
                    }
                }
                for (int k = 0; k < 8; k++)
                {
                    int num40 = (int)material * 8 + k;
                    _incomingCount[num40] = 0;
                    _outgoingCount[num40] = 0;
                }
                _incomingAmount[(int)material] = 0;
                _outgoingAmount[(int)material] = 0;
            }
        }
    }
}
