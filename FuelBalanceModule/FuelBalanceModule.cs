using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FuelBalancer
{
    public class ModuleFuelBalancer : PartModule
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
            Debug.Log("BindFuelLine():");
            if (this.part is FuelLine)
            {
                Debug.Log("Fuel line found.");
                m_fuelLine = this.part as FuelLine;
                if (m_fuelLine.parent.Resources.Contains(resourceName))
                {
                    Debug.Log("Resource: " + resourceName + " found on source part.");
                    m_sourceTank = m_fuelLine.parent;
                }
                if (m_fuelLine.target.Resources.Contains(resourceName))
                {
                    Debug.Log("Resource: " + resourceName + " found on target part.");
                    m_targetTank = m_fuelLine.target;
                }
            }
        }

        private void CheckActivated()
        {
            if (m_activated == false) return;
            if (m_fuelLine != null && m_sourceTank != null && m_targetTank != null) return;

            m_activated = false;
        }

        [KSPEvent(name = "ContextMenuActivateBalancer", active = true, guiActive = true, guiName = "Enable Balancer")]
        public void ContextMenuActivateBalancer()
        {
            m_activated = true;
            BindFuelLine();
            CheckActivated();
        }
        [KSPEvent(name = "ContextMenuDeactivateBalancer", active = false, guiActive = true, guiName = "Disable Balancer")]
        public void ContextMenuDeactivateBalancer()
        {
            m_activated = false;
        }

        [KSPAction("Activate Balancer", actionGroup = KSPActionGroup.None)]
        public void ActionGroupActivateBalancer()
        {
            ContextMenuActivateBalancer();
        }
        [KSPAction("Deactivate Balancer", actionGroup = KSPActionGroup.None)]
        public void ActionGroupDeactivateBalancer()
        {
            ContextMenuDeactivateBalancer();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            m_startState = state;
            this.Events["ContextMenuActivateBalancer"].guiName = "Enable " + resourceName + " Balancer";
            this.Events["ContextMenuDeactivateBalancer"].guiName = "Disable " + resourceName + " Balancer";
        }

        public override void OnUpdate()
        {
            Debug.Log("OnFixedUpdate(): " + m_startState.ToString());
            if (m_startState == StartState.Editor) return;

            BindFuelLine();
            CheckActivated();

            Debug.Log("Activated: " + m_activated.ToString());
            if (m_activated)
            {
                this.Events["ContextMenuActivateBalancer"].active = false;
                this.Events["ContextMenuDeactivateBalancer"].active = true;
                double srcAmount = m_sourceTank.Resources[resourceName].amount;
                double srcMaxAmount = m_sourceTank.Resources[resourceName].maxAmount;
                double tgtAmount = m_targetTank.Resources[resourceName].amount;
                double tgtMaxAmount = m_targetTank.Resources[resourceName].maxAmount;

                double tgtRatio = tgtAmount / tgtMaxAmount;
                double srcRatio = srcAmount / srcMaxAmount;

                double desiredRatio = (srcAmount + tgtAmount) / (srcMaxAmount + tgtMaxAmount);

                Debug.Log(srcRatio.ToString() + " " + desiredRatio.ToString() + " " + tgtRatio.ToString());

                if (tgtRatio > srcRatio)
                {
                    // tgtRatio > desiredRatio > srcRatio
                    // Direction: src <- tgt
                    double tgtAmountToTransfer = (tgtRatio - desiredRatio) * tgtMaxAmount;
                    if (tgtAmountToTransfer > maxFlowRate * Time.fixedDeltaTime)
                    {
                        m_sourceTank.Resources[resourceName].amount += maxFlowRate * Time.fixedDeltaTime;
                        m_targetTank.Resources[resourceName].amount -= maxFlowRate * Time.fixedDeltaTime;
                        Debug.Log("Transferred: " + (maxFlowRate * Time.fixedDeltaTime).ToString());
                    }
                    else
                    {
                        m_sourceTank.Resources[resourceName].amount += tgtAmountToTransfer;
                        m_targetTank.Resources[resourceName].amount -= tgtAmountToTransfer;
                        Debug.Log("Transferred: " + (tgtAmountToTransfer).ToString());
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
                        Debug.Log("Transferred: " + (maxFlowRate * Time.fixedDeltaTime).ToString());
                    }
                    else
                    {
                        m_targetTank.Resources[resourceName].amount += srcAmountToTransfer;
                        m_sourceTank.Resources[resourceName].amount -= srcAmountToTransfer;
                        Debug.Log("Transferred: " + (srcAmountToTransfer).ToString());
                    }
                }
            }
            else
            {
                this.Events["ContextMenuActivateBalancer"].active = true;
                this.Events["ContextMenuDeactivateBalancer"].active = false;
            }

            base.OnUpdate();
        }
    }
}
