using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FuelBalanceModule
{
	public class ModuleFuelRouteCutoff : PartModule
	{
		private StartState m_startState = StartState.None;

		private FuelLine m_fuelLine = null;
		private Part m_sourceTank = null;
		private Part m_targetTank = null;

		private List<string> resources = new List<string>();		
		
		[KSPField]
		public string resourceName;
		[KSPField]
		public float cutOffThreshold = 0.0f;

		private void BindFuelLine()
		{
			m_fuelLine = null;
			m_sourceTank = null;
			m_targetTank = null;
			Debug.Log("BindFuelLine():");
			if (this.part != null)
			{
				if (this.part is FuelLine)
				{
					Debug.Log("Fuel line found.");
					m_fuelLine = this.part as FuelLine;

					if (m_fuelLine.parent != null)
					{
						foreach (string resource in resources)
						{
							if (m_fuelLine.parent.Resources.Contains(resource))
							{
								Debug.Log("Resource: " + resource + " found on source part.");
								m_sourceTank = m_fuelLine.parent;
							}
						}
					}
					if (m_fuelLine.target != null)
					{
						foreach (string resource in resources)
						{
							if (m_fuelLine.target.Resources.Contains(resource))
							{
								Debug.Log("Resource: " + resource + " found on target part.");
								m_targetTank = m_fuelLine.target;
							}
						}
					}
				}
			}
		}

		private bool CheckActivated()
		{
			if (m_fuelLine != null && m_sourceTank != null && m_targetTank != null) return true;
			return false;
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);

			m_startState = state;

			resources.Clear();
			string[] resourceList = resourceName.Split(new string[] {",", " "}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string resource in resourceList)
			{
				resources.Add(resource);
				Debug.Log("Resource: " + resource);
			}
		}

		public override void OnUpdate()
		{
			if (m_startState != StartState.Editor)
			{
				BindFuelLine();

				if (CheckActivated())
				{
					double minSrcRatio = 1.0f;
					foreach (string resource in resources)
					{
						double srcAmount = m_sourceTank.Resources[resource].amount;
						double srcMaxAmount = m_sourceTank.Resources[resource].maxAmount;

						double srcRatio = srcAmount / srcMaxAmount;

						if (minSrcRatio > srcRatio)
							minSrcRatio = srcRatio;
					}
					
					Debug.Log(minSrcRatio.ToString() + " " + cutOffThreshold.ToString());

					if (minSrcRatio < cutOffThreshold)
					{
						// We need to cut-off the fuel line.
						Debug.Log("Fuel line closed.");
						m_fuelLine.CloseFuelLine();
						//m_fuelLine.BreakLine();
					}
				}
			}
			base.OnUpdate();
		}
	}
}
