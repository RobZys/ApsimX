﻿using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using Models.Soils;
using Models.PMF;

namespace Models.SurfaceOM
{

    public partial class SurfaceOrganicMatter
    {

        [EventSubscribe("Tick")]
        private void OnTick(object sender, EventArgs e)
        {
            if (initialised)
                surfaceOM_ONtick();
        }
        bool initialised = false;

        public void OnTillage(TillageType data)
        {
            surfom_tillage(data);
        }

        public class Add_surfaceomType
        {
            public string name = "";
            public string type = "";
            public double mass;
            public double n;
            public double cnr;
            public double p;
            public double cpr;
        }

        [EventSubscribe("AddSurfaceOM")]
        private void OnAdd_surfaceom(Add_surfaceomType data)
        {
            surfom_add_surfom(data);
        }

        [EventSubscribe("Initialised")]
        private void OnInitialised(object sender, EventArgs e) { OnReset(); }

        [EventSubscribe("Reset")]
        private void OnReset() { initialised = true; surfom_Reset(); }

        [EventSubscribe("RemoveSurfaceOM")]
        private void OnRemove_surfaceOM(SurfaceOrganicMatterType SOM) { surfom_remove_surfom(SOM); }

        [EventSubscribe("NewMet")]
        private void OnNewmet(Models.WeatherFile.NewMetType newmetdata) { g.MetData = newmetdata; }

        public class IrrigationApplicationType : EventArgs
        {
            public double Amount;
            public bool will_runoff;
            public double Depth;
            public double NO3;
            public double NH4;
            public double CL;
        }

        [EventSubscribe("Irrigated")]
        private void OnIrrigated(object sender, IrrigationApplicationType data) { surfom_ONirrigated(data); }

        public class CropChoppedType
        {
            public string crop_type = "";
            public string[] dm_type;
            public double[] dlt_crop_dm;
            public double[] dlt_dm_n;
            public double[] dlt_dm_p;
            public double[] fraction_to_residue;
        }


        [EventSubscribe("CropChopped")]
        private void OnCrop_chopped(CropChoppedType data) { surfom_ON_Crop_chopped(data); }

        [EventSubscribe("BiomassRemoved")]
        private void OnBiomassRemoved(BiomassRemovedType BiomassRemoved) { SurfOMOnBiomassRemoved(BiomassRemoved); }

        [EventSubscribe("WaterMovementCompleted")]
        private void OnWaterMovementCompleted(object sender, EventArgs e)
        {
            surfom_get_other_variables();
            surfom_Process();
            //catch (Exception e) { }

            if (Today.DayOfYear == 300)
                return;
        }

        [EventSubscribe("ActualResidueDecompositionCalculated")]
        private void OnActualResidueDecompositionCalculated(SurfaceOrganicMatterDecompType SOMDecomp) { surfom_decompose_surfom(SOMDecomp); }

        public class Prop_upType
        {
            public string name = "";
            public double standing_fract;
        }
        [EventSubscribe("PropUp")]
        private void OnPropUp(Prop_upType data) { surfom_prop_up(data); }

        public class AddFaecesType
        {
            public double Defaecations;
            public double VolumePerDefaecation;
            public double AreaPerDefaecation;
            public double Eccentricity;
            public double OMWeight;
            public double OMN;
            public double OMP;
            public double OMS;
            public double OMAshAlk;
            public double NO3N;
            public double NH4N;
            public double POXP;
            public double SO4S;
        }


        [EventSubscribe("AddFaeces")]
        private void OnAddFaeces(AddFaecesType data) { surfom_add_faeces(data); }
    }

}