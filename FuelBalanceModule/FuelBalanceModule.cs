using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FuelBalanceModule
{
    public class FuelBalanceModule : PartModule
    {
        private StartState m_startState = StartState.None;

        private FuelLine m_fuelLine = null;
        private Part m_sourceTank = null;
        private Part m_targetTank = null;

        [KSPField]
        public string resourceName;
        [KSPField]
        public float maxFlowRate = 100.0f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Balance Module")]
        private bool m_activated = false;

        private void BindFuelLine()
        {
            m_fuelLine = null;
            m_sourceTank = null;
            m_targetTank = null;
            if (this.part is FuelLine)
            {
                m_fuelLine = this.part as FuelLine;
                if (m_fuelLine.parent.Resources.Contains(resourceName))
                    m_sourceTank = m_fuelLine.parent;
                if (m_fuelLine.target.Resources.Contains(resourceName))
                    m_targetTank = m_fuelLine.target;
            }
        }

        private void CheckActivated()
        {
            if (m_activated == false) return;
            if (m_fuelLine != null && m_sourceTank != null && m_targetTank != null) return;

            m_activated = false;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            m_startState = state;
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (m_startState == StartState.Editor) return;

            BindFuelLine();
            CheckActivated();

            if(m_activated)
            {
                double srcAmount = m_sourceTank.Resources[resourceName].amount;
                double srcMaxAmount = m_sourceTank.Resources[resourceName].maxAmount;
                double tgtAmount = m_targetTank.Resources[resourceName].amount;
                double tgtMaxAmount = m_targetTank.Resources[resourceName].maxAmount;

                double tgtRatio = tgtAmount / tgtMaxAmount;
                double srcRatio = srcAmount / srcMaxAmount;

                double desiredRatio = (srcAmount + tgtAmount) / (srcMaxAmount + tgtMaxAmount);
                if (tgtRatio > srcRatio)
                {
                    // tgtRatio > desiredRatio > srcRatio
                    // Direction: src <- tgt
                    double tgtAmountToTransfer = (tgtRatio - desiredRatio) * tgtMaxAmount;
                    if (tgtAmountToTransfer > maxFlowRate * Time.fixedDeltaTime)
                    {
                        m_sourceTank.Resources[resourceName].amount += maxFlowRate * Time.fixedDeltaTime;
                        m_targetTank.Resources[resourceName].amount -= maxFlowRate * Time.fixedDeltaTime;
                    }
                    else
                    {
                        m_sourceTank.Resources[resourceName].amount += tgtAmountToTransfer;
                        m_targetTank.Resources[resourceName].amount -= tgtAmountToTransfer;
                    }
                }
                else if (srcRatio > tgtRatio)
                {
                    // srcRatio > desiredRatio > tgtRatio
                    // Direction: tgt <- src
                    double srcAmountToTransfer = (srcRatio - desiredRatio) * srcMaxAmount;
                    if (srcAmountToTransfer > maxFlowRate * Time.fixedDeltaTime)
                    {
                        m_targetTank.Resources[resourceName].amount += maxFlowRate * Time.fixedDeltaTime;
                        m_sourceTank.Resources[resourceName].amount -= maxFlowRate * Time.fixedDeltaTime;
                    }
                    else
                    {
                        m_targetTank.Resources[resourceName].amount += srcAmountToTransfer;
                        m_sourceTank.Resources[resourceName].amount -= srcAmountToTransfer;
                    }
                }
            }
        }
    }
}
