using System.Collections.Immutable;
using LpjGuess.Core.Models;
using LpjGuess.Core.Models.Entities;
using LpjGuess.Core.Models.Importer;

namespace LpjGuess.Core.Services;

/// <summary>
/// Provides metadata about known output file types
/// </summary>
public static class OutputFileDefinitions
{
    private static readonly ImmutableDictionary<string, OutputFileMetadata> Definitions;

    static OutputFileDefinitions()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, OutputFileMetadata>();

        // Daily gridcell-level outputs
        AddPftOutput(builder, "file_agb", "AGB", "Above-Ground Biomass", "kg/m2", AggregationLevel.Gridcell, TemporalResolution.Daily);
        AddPftOutput(builder, "file_agb_tree", "AGB", "Above-Ground Tree Biomass", "kg/m2", AggregationLevel.Gridcell, TemporalResolution.Daily);

        // Daily PFT-level outputs
        AddPftOutput(builder, "file_dave_lai", "LAI", "Leaf Area Index", "m2/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_fpc", "FPC", "Foliar Projective Cover", "", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_crownarea", "Crown Area", "Crown Area", "m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_agd_g", "Gross Photosynthesis", "Gross Photosynthesis", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_rd_g", "Leaf Respiration", "Leaf Respiration", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_je", "PAR-limited Photosynthesis", "PAR-limited photosynthetic rate", "gC/m2/h", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_vm", "RuBisCO", "RuBisCO capacity", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_fphen_activity", "Phenology Activity", "Dormancy Downregulation", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_fdev_growth", "Development Factor", "Development Factor for growth demand", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_frepr_cstruct", "Reproductive Ratio", "Ratio of reproductive to aboveground structural biomass", "", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_growth_demand", "Growth Demand", "Growth Demand", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_transpiration", "Transpiration", "Transpiration", "mm/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nscal", "N Stress", "Nitrogen Stress (1=no stress)", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nscal_mean", "N Stress", "Nitrogen Stress (5 day running mean)", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ltor", "Leaf:Root Ratio", "Leaf:Root Ratio", "", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cue", "CUE", "Carbon Use Efficiency", "", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_alpha_leaf", "Leaf Alpha", "Leaf Sink Strength", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_alpha_root", "Root Alpha", "Root Sink Strength", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_alpha_sap", "Sap Alpha", "Sap Sink Strength", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_alpha_repr", "Reproductive Alpha", "Reproductive Sink Strength", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass", "C Mass", "PFT-Level Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_leaf_limit", "Leaf Limit", "Optimum Leaf C Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_root_limit", "Root Limit", "Optimum Root C Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_sap_limit", "Sap Limit", "Optimum Sap C Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_repr_limit", "Reproductive Limit", "Optimum Reproductive C Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_storage_limit", "Storage Limit", "Optimum Storage C Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cgrow_leaf", "Leaf Growth", "Leaf Carbon Allocation", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cgrow_root", "Root Growth", "Root Carbon Allocation", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cgrow_sap", "Sap Growth", "Sap Carbon Allocation", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cgrow_repr", "Reproductive Growth", "Reproductive Carbon Allocation", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_diameter_inc", "Diameter Increment", "Diameter Increment", "m/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_height_inc", "Height Increment", "Height Increment", "m/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_height", "Height", "Plant Height", "m", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_diameter", "Diameter", "Stem Diameter", "m", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_basalarea", "Basal Area", "Basal Area", "m2/tree", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_basalarea_inc", "Basal Area Increment", "Basal Area Increment", "m2/tree/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_dturnover_leaf", "Leaf Turnover", "Leaf C Turnover", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_dturnover_root", "Root Turnover", "Root C Turnover", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_dturnover_sap", "Sap Turnover", "Sapwood C Turnover", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_anc_frac", "ANC Fraction", "Fraction of Photosynthesis Limited by Rubisco", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_anj_frac", "ANJ Fraction", "Fraction of Photosynthesis Limited by RuBP Regeneration", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_anp_frac", "ANP Fraction", "Fraction of Photosynthesis Limited by TPU", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_dnuptake", "N Uptake", "Nitrogen Uptake", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cexcess", "Carbon Overflow", "Carbon Overflow", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ctolitter_leaf", "Leaf C to Litter", "Leaf Carbon to Litter", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ntolitter_leaf", "Leaf N to Litter", "Leaf Nitrogen to Litter", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ctolitter_root", "Root C to Litter", "Root Carbon to Litter", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ntolitter_root", "Root N to Litter", "Root Nitrogen to Litter", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ctolitter_repr", "Reproductive C to Litter", "Reproductive Carbon to Litter", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ntolitter_repr", "Reproductive N to Litter", "Reproductive Nitrogen to Litter", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ctolitter_crown", "Crown C to Litter", "Crown Carbon to Litter", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ntolitter_crown", "Crown N to Litter", "Crown Nitrogen to Litter", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_aboveground_cmass", "Above-Ground C Mass", "Above-Ground Carbon Biomass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_belowground_cmass", "Below-Ground C Mass", "Below-Ground Carbon Biomass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_aboveground_nmass", "Above-Ground N Mass", "Above-Ground Nitrogen Biomass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_belowground_nmass", "Below-Ground N Mass", "Below-Ground Nitrogen Biomass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_aboveground_tree_biomass", "Above-Ground Tree Biomass", "Above-Ground Tree Biomass", "kg/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_live_biomass", "Live Biomass", "Live Biomass", "kg/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_indiv_npp", "NPP", "Net Primary Productivity", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_sla", "SLA", "Specific Leaf Area", "m2/kgC", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_leaf", "Leaf C Mass", "Green Leaf Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_leaf_brown", "Brown Leaf C Mass", "Brown Leaf Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_leaf", "Leaf N Mass", "Green Leaf Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_crown", "Crown C Mass", "Crown Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_repr", "Reproductive C Mass", "Reproductive Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_root", "Root C Mass", "Root Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_root", "Root N Mass", "Root Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass", "N Mass", "Vegetation N Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_storage", "Storage C Mass", "Non-Structural Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_storage_max", "Storage C Capacity", "Non-Structural Carbon Capacity", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_storage", "Storage N Mass", "Non-Structural Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_storage_max", "Max Storage N Mass", "Max Non-Structural Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_sap", "Sapwood C Mass", "Sapwood Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_sap", "Sapwood N Mass", "Sapwood Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_heart", "Heartwood C Mass", "Heartwood Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_heart", "Heartwood N Mass", "Heartwood Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_repr", "Reproductive N Mass", "Reproductive Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_ndemand", "N Demand", "Nitrogen Demand", "kgN/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_density", "Density", "Tree Density", "/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_sapwood_area", "Sapwood Area", "Sapwood Area", "m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_latosa", "LA:SA", "Leaf Area to Sapwood Area Ratio", "", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_fpar", "FPAR", "Fraction of Absorbed PAR", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_indiv_gpp", "GPP", "Gross Primary Productivity", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_resp_autotrophic", "Autotrophic Respiration", "Autotrophic Respiration", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_resp_maintenance", "Maintenance Respiration", "Maintenance Respiration", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_resp_growth", "Growth Respiration", "Growth Respiration", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_layerwise_fpar", "FPAR", "Layerwise Fraction of Absorbed PAR", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_layerwise_lai", "LAI", "Layerwise Leaf Area Index", "m2/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_wscal", "Water Stress", "Water Stress (1=no stress)", "0-1", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_litter_repr", "Reproductive C Litter", "Reproductive Carbon in Litter", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_nmass_litter_repr", "Reproductive N Litter", "Reproductive Nitrogen in Litter", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_dresp", "Autotrophic Respiration", "Autotrophic Respiration by PFT", "kgC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cmass_seed_ext", "Grass Seedbank", "Grass Seedbank Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_subdaily_an", "Net Photosynthesis", "Net Photosynthesis", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_rd", "Leaf Respiration", "Leaf Respiration", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_anc", "RuBisCO-limited Photosynthesis", "RuBisCO-limited Photosynthesis", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_anj", "RuBP-Limited Photosynthesis", "RuBP Regeneration-Limited Photosynthesis", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_gsw", "Stomatal Conductance", "Stomatal conductance to Water Vapour", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_ci", "CO2 Concentration", "Intercellular CO2 Concentration", "mol/mol", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_vcmax", "Vcmax", "Maximum Carboxylation Rate", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_subdaily_jmax", "Jmax", "Maximum Electron Transport Rate", "mol/m2/s", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_sw", "Soil Water", "Soil Water Fraction Full", "mm", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_swmm", "Soil Water", "Soil Water Content", "mm", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_swvol", "Soil Water", "Volumetric Soil Water Content", "m3/m3", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cfluxes_patch", "Patch C Fluxes", "Daily patch-level carbon fluxes", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_cfluxes_pft", "PFT C Fluxes", "Daily PFT-level carbon fluxes", "gC/m2/day", AggregationLevel.Patch, TemporalResolution.Daily);
        AddPftOutput(builder, "file_dave_anetps_ff_max", "Max Forest Floor Net Photosynthesis", "Maximum Recorded Annual Net Forest-Floor Photosynthesis", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Daily);

        // These aren't really PFT outputs, but they have dynamic column names
        // (one column per timestep). Therefore, for now I'm treating them as
        // PFT outputs.
        AddPftOutput(builder, "file_dave_met_subdaily_temp", "Temperature", "Air Temperature", "°C", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_par", "PAR", "Photosynthetically Active Radiation", "kJ/m2/timestep", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_vpd", "VPD", "Vapor Pressure Deficit", "kPa", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_insol", "Insolation", "Insolation (units depend on instype)", "kJ/m2/timestep", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_precip", "Precipitation", "Precipitation", "mm", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_pressure", "Pressure", "Atmospheric Pressure", "kPa", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddPftOutput(builder, "file_dave_met_subdaily_co2", "CO2", "Atmospheric CO2 Concentration", "ppm", AggregationLevel.Patch, TemporalResolution.Subdaily);
        AddOutput(builder, "file_dave_met_pressure", "Pressure", "Atmospheric pressure", "kPa", ["pressure"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_co2", "CO2", "Atmospheric CO2 concentration", "ppm", ["co2"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_temp", "Temperature", "Air Temperature", "°C", ["temp"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_par", "PAR", "Photosynthetically Active Radiation", "kJ/m2/timestep", ["par"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_vpd", "VPD", "Vapor Pressure Deficit", "kPa", ["vpd"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_insol", "Insolation", "Insolation", "", ["insol"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_met_precip", "Precipitation", "precipitation", "mm", ["precip"], AggregationLevel.Patch, TemporalResolution.Daily);

        // Daily individual-level outputs.
        AddOutput(builder, "file_dave_indiv_cpool", "C Pools", "Vegetation Carbon Pools", "kgC/m2", ["cmass_leaf", "cmass_root", "cmass_crown", "cmass_sap", "cmass_heart", "cmass_repr", "cmass_storage"], AggregationLevel.Individual, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_indiv_npool", "N Pools", "Vegetation Nitrogen Pools", "kgN/m2", ["nmass_leaf", "nmass_root", "nmass_crown", "nmass_sap", "nmass_heart", "nmass_repr", "nmass_storage"], AggregationLevel.Individual, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_indiv_lai", "LAI", "Leaf Area Index", "m2/m2", ["lai"], AggregationLevel.Individual, TemporalResolution.Daily);

        // Annual patch-level outputs
        AddOutput(builder, "file_dave_patch_age", "Patch Age", "Time Since Disturbance", "years", ["age"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_arunoff", "Runoff", "Annual Runoff", "mm", ["runoff"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_globfirm", "Globfirm", "Annual GLOBFIRM Outputs", [
            ("fireprob", "0-1")
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_acpool", "C Pools", "Carbon Pools", "kgC/m2", [
            "cmass_veg",
            "cmass_litter",
            "cmass_soil",
            "total"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_anpool", "N Pools", "Nitrogen Pools", "kgN/m2", [
            "nmass_veg",
            "nmass_litter",
            "nmass_soil",
            "total"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_acflux", "C Fluxes", "Carbon Fluxes", "gC/m2", [
            "npp",
            "gpp",
            "ra",
            "rh"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddMonthlyOutput(builder, "file_dave_mwcont_upper", "Water Content", "Water Content Fraction of Upper Soil Layer", "0-1", AggregationLevel.Patch);
        AddMonthlyOutput(builder, "file_dave_mwcont_lower", "Water Content", "Water Content Fraction of Lower Soil Layer", "0-1", AggregationLevel.Patch);
        AddOutput(builder, "file_dave_apet", "PET", "Potential Evapotranspiration", "mm", ["pet"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_asimfire", "Simfire", "Simfire analysis", [
            ("burned_area", "fraction"),
            ("fire_carbon", "gC/m2") 
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_afuel", "Fuel", "Blaze fuel availability", "gC/m2", ["fuel"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_acoarse_woody_debris", "CWD", "Coarse Woody Debris", "gC/m2", ["cwd"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_amet_year", "Met Year", "Current year of met data being used", "year", ["year"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_aco2", "CO2", "Atmospheric CO2 Concentration", "ppm", ["co2"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_aminleach", "Mineral N Leaching", "Leaching of Soil Mineral N", "kgN/m2/yr", ["aminleach"], AggregationLevel.Patch, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_sompool_acmass", "SOM Pool C Mass", "Surface Organic Matter Carbon Mass by Pool", "kgC/m2", [
            "SURFSTRUCT",
            "SOILSTRUCT",
            "SOILMICRO",
            "SOILACTIVE",
            "SOILSLOW",
            "SOILPASSIVE",
            "SURFMETA",
            "SURFMICRO",
            "SURFFWD",
            "SURFCWD",
            "total"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_sompool_anmass", "SOM Pool N Mass", "Surface Organic Matter Nitrogen Mass by Pool", "kgN/m2", [
            "SURFSTRUCT",
            "SOILSTRUCT",
            "SOILMICRO",
            "SOILACTIVE",
            "SOILSLOW",
            "SOILPASSIVE",
            "SURFMETA",
            "SURFMICRO",
            "SURFFWD",
            "SURFCWD",
            "total"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_andep", "N Deposition", "Nitrogen Deposition", "kgN/m2", [
            "dNO3dep",
            "dNH4dep",
            "nfert",
            "total"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        AddOutput(builder, "file_dave_anfixation", "N Fixation", "Nitrogen Fixation", "kgN/m2", [
            "nfixation"
        ], AggregationLevel.Patch, TemporalResolution.Annual);

        // Daily patch-level outputs
        AddOutput(builder, "file_dave_daylength", "Day Length", "Day Length", "h", ["daylength"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_soil_nmass_avail", "Available Soil N", "Soil N Mass Available for Plant Uptake", "kgN/m2", ["soil_nmass_avail"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_dsimfire", "Simfire", "Simfire Analysis", [
            ("burned_area", "fraction"),
            ("fire_carbon", "gC/m2") 
        ], AggregationLevel.Patch, TemporalResolution.Daily);

        AddOutput(builder, "file_dave_sompool_cmass", "SOM Pool C Mass", "Surface Organic Matter Carbon Mass by Pool", "kgC/m2", ["cmass"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_sompool_nmass", "SOM Pool N Mass", "Surface Organic Matter Nitrogen Mass by Pool", "kgN/m2", ["nmass"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_ninput", "N Input", "Nitrogen Deposition", "kgN/m2", ["ninput"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_fpar_ff", "Forest-Floor FPAR", "Fraction of Photosynthetically Active Radiation Reaching the Forest Floor", "0-1", ["fpar_ff"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_aet", "ET", "Evaporation and Transpiration", "mm", ["evap", "transp"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_resp_heterotrophic", "Heterotrophic Respiration", "Heterotrophic respiration", "gC/m2/day", ["resp_h"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_resp", "Respiration", "Ecosystem Respiration", "gC/m2/day", ["resp"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_gpp", "GPP", "Gross Primary Productivity", "gC/m2/day", ["gpp"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_npp", "NPP", "Net Primary Productivity", "gC/m2/day", ["npp"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_nee", "NEE", "Net Ecosystem Exchange", "gC/m2/day", ["nee"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_evaporation", "Evaporation", "Evaporation", "mm/day", ["evaporation", "interception", "transpiration", "evapotranspiration"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_soilc", "Soil Carbon", "Soil Carbon Carbon Mass", "kgC/m2", ["soilc"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_soiln", "Soil Nitrogen", "Soil Nitrogen Nitrogen Mass", "kgN/m2", ["soiln"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_soil_nflux", "Soil N Flux", "Soil Nitrogen Flux", "kgN/m2/day", ["nflux"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_dfuel", "Fuel Availability", "Fuel Availability for Blaze", "kgC/m2", ["fuel"], AggregationLevel.Patch, TemporalResolution.Daily);
        AddOutput(builder, "file_dave_dcoarse_woody_debris", "CWD", "Coarse Woody Debris Carbon Mass", "gC/m2", ["cwd"], AggregationLevel.Patch, TemporalResolution.Daily);

        // Annual patch-level PFT outputs.
        AddPftOutput(builder, "file_dave_alai", "LAI", "Leaf Area Index", "m2/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_afpc", "FPC", "Foliage Projective Cover", "0-1", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acmass", "C Mass", "Carbon Mass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_anmass", "N Mass", "Nitrogen Mass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_aheight", "Height", "Plant Height", "m", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_aaet", "AET", "Actual Evapotranspiration", "mm", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_adensity", "Density", "Density of individuals over patch", "/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_altor", "Leaf:Root Ratio", "Leaf:Root Ratio", "", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_anuptake", "N Uptake", "Vegetation Nitrogen Uptake", "kgN/m2/year", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_a_aboveground_cmass", "Above-Ground C Mass", "Above-Ground Carbon biomass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_a_belowground_cmass", "Below-Ground C Mass", "Below-Ground Carbon biomass", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_a_aboveground_nmass", "Above-Ground N Mass", "Above-Ground Nitrogen biomass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_a_belowground_nmass", "Belowground N Mass", "Belowground Nitrogen biomass", "kgN/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_anpp", "NPP", "Net Primary Productivity", "kgC/m2/year", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_agpp", "GPP", "Gross Primary Productivity", "kgC/m2/year", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_aresp", "Respiration", "Autotrophic Respiration", "kgC/m2/year", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acmass_mort", "Mortality C Mass", "Carbon Mass of Killed Vegetation", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_aclitter", "C Litter", "Carbon Litter", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_ancohort", "Number of Cohorts", "Number of Cohorts of each PFT", "count", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_anetps_ff", "Forest-Floor Net Photosynthesis", "Net photosynthesis at forest floor", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acalloc_leaf", "Leaf C Allocation", "Carbon Allocation to Leaf", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acalloc_root", "Root C Allocation", "Carbon Allocation to Root", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acalloc_repr", "Repr C Allocation", "Carbon Allocation to Repr", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acalloc_sap", "Sapwood C Allocation", "Carbon Allocation to Sapwood", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dave_acalloc_crown", "Crown C Allocation", "Carbon Allocation to Crown", "kgC/m2", AggregationLevel.Patch, TemporalResolution.Annual);

        // Annual stand-level outputs
        AddOutput(builder, "file_dave_stand_frac", "Stand Fraction", "Fraction of the gridcell occupied by each stand", "", ["fraction"], AggregationLevel.Stand, TemporalResolution.Annual);
        AddOutput(builder, "file_dave_stand_type", "Stand Type", "Stand landcover types", "", ["type"], AggregationLevel.Stand, TemporalResolution.Annual);

        // Annual gridcell-level PFT outputs.
        AddPftOutput(builder, "file_cmass", "C Mass", "Total Carbon Biomass", "kgC/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_anpp", "NPP", "Net Primary Production", "kgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_agpp", "GPP", "Gross Primary Production", "kgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_fpc", "FPC", "Foliage Projective Cover", "0-1", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_aaet", "AET", "Actual Evapotranspiration", "mm/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_lai", "LAI", "Leaf Area Index", "m2/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);

        // Annual gridcell-level outputs.
        AddOutput(builder, "file_cflux", "Carbon Fluxes", "Carbon Fluxes", "kgC/m2/year", [
            "Veg",
            "Repr",
            "Soil",
            "Fire",
            "Est",
            "Seed",
            "Harvest",
            "LU_ch",
            "Slow_h",
            "NEE"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddPftOutput(builder, "file_doc", "Dissolved Organic Carbon", "Dissolved Organic Carbon", "kgC/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_dens", "Tree Density", "Tree Density", "indiv/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddOutput(builder, "file_cpool", "Soil Carbon", "Soil Carbon Pools", "kgC/m2", [
            "VegC",
            "LitterC",
            "SoilC",
            "Total"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddPftOutput(builder, "file_clitter", "Carbon Litter", "Carbon in litter", "kgC/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddOutput(builder, "file_runoff", "Runoff", "Runoff", "mm/year", [
            "Surf",
            "Drain",
            "Base",
            "Total"], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddOutput(builder, "file_wetland_water_added", "Wetland Water Added", "Water Added to Wetland", "mm", ["H2OAdded"], AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_speciesheights", "Species Height", "Mean Species Height", "m", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_speciesdiam", "Species Diameter", "Mean Species Diameter", "m", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddOutput(builder, "file_firert", "Fire Return Time", "Fire Return Time", [
            ("FireRT", "years"),
            ("BurntFr", "0-1")], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddPftOutput(builder, "file_nmass", "N Mass", "Nitrogen Content in Vegetation", "kgN/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_cton_leaf", "Leaf C:N Ratio", "Carbon to Nitrogen Ratio in Leaves", "-", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddOutput(builder, "file_nsources", "N Sources", "Nitrogen Sources", "kgN/m2/year", [
            "NH4dep",
            "NO3dep",
            "fix",
            "fert",
            "input",
            "min",
            "imm",
            "netmin",
            "Total"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddOutput(builder, "file_npool", "N Pools", "Nitrogen Pools", "kgN/m2", [
            "VegN",
            "LitterN",
            "SoilN",
            "Total"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddPftOutput(builder, "file_nlitter", "N Litter", "Litter Nitrogen Mass", "kgN/m2", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_nuptake", "N Uptake", "Nitrogen Uptake", "kgN/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_vmaxnlim", "Vmax N Limitation", "Nitrogen Limitation on Vmax", "", AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddOutput(builder, "file_nflux", "N Fluxes", "Nitrogen Fluxes", "kgN/m2/year", [
            "NH4dep",
            "NO3dep",
            "fix",
            "fert",
            "est",
            "flux",
            "leach",
            "NEE",
            "Total"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddOutput(builder, "file_ngases", "N Gas Emissions", "Nitrogen Gas Emissions", "kgN/m2/year", [
            "NH3_fire",
            "NH3_soil",
            "NOx_fire",
            "NOx_soil",
            "N2O_fire",
            "N2O_soil",
            "N2_fire",
            "N2_soil",
            "Total"
        ], AggregationLevel.Gridcell, TemporalResolution.Annual);

        AddPftOutput(builder, "file_aiso", "Isoprene Flux", "Isoprene Flux", "mgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_amon", "Monoterpene Flux", "Monoterpene Flux", "mgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_amon_mt1", "Endocyclic Monoterpene Flux", "Endocyclic Monoterpene Flux", "mgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddPftOutput(builder, "file_amon_mt2", "Other Monoterpene Flux", "Other Monoterpene Flux", "mgC/m2/year", AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddOutput(builder, "file_aburned_area_out", "BLAZE Burned Area", "BLAZE Burned Area", [("BurntFr", "0-1")], AggregationLevel.Gridcell, TemporalResolution.Annual);
        AddOutput(builder, "file_simfireanalysis_out", "SIMFIRE Analytics", "SIMFIRE Analytics", [
            ("Biome", @"0=NOVEG, 1=CROP, 2=NEEDLELEAF, 3=BROADLEAF, 4=MIXED_FOREST, 5=SHRUBS, 6=SAVANNA, 7=TUNDRA, 8=BARREN"),
            ("MxNest", "-"),
            ("PopDens", "inhabitants/km2"),
            ("AMxFApar", "0-1"),
            ("FireProb", "0-1"),
            ("Region", "unused")], AggregationLevel.Gridcell, TemporalResolution.Annual);

        // Monthly gridcell-level outputs
        AddMonthlyOutput(builder, "file_mnpp", "NPP", "Net Primary Productivity", "kgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mlai", "LAI", "Leaf Area Index", "m2/m2", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mgpp", "GPP", "Gross Primary Productivity", "kgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mra", "Ra", "Autotrophic Respiration", "kgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_maet", "AET", "Actual Evapotranspiration", "mm/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mpet", "PET", "Potential Evapotranspiration", "mm/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mevap", "Evaporation", "Evaporation", "mm/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mrunoff", "Runoff", "Runoff", "mm/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mintercep", "Interception", "Interception", "mm/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mrh", "Rh", "Heterotrophic Respiration", "kgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mnee", "NEE", "Net Ecosystem Exchange", "kgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mwcont_upper", "Water Content", "Water Content Fraction of Upper Soil Layer", "", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mwcont_lower", "Water Content", "Water Content Fraction of Lower Soil Layer", "", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_miso", "Isoprene Flux", "Isoprene Flux", "mgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mmon", "Monoterpene Flux", "Monoterpene Flux", "mgC/m2/month", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mmon_mt1", "Endocyclic Monoterpene Flux", "Endocyclic Monoterpene Flux", "mgC/m2", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mmon_mt2", "Other Monoterpene Flux", "Other Monoterpene Flux", "mgC/m2", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth5", "Soil Temperature", "Soil temperature (5cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth15", "Soil Temperature", "Soil temperature (15cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth25", "Soil Temperature", "Soil temperature (25cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth35", "Soil Temperature", "Soil temperature (35cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth45", "Soil Temperature", "Soil temperature (45cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth55", "Soil Temperature", "Soil temperature (55cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth65", "Soil Temperature", "Soil temperature (65cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth75", "Soil Temperature", "Soil temperature (75cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth85", "Soil Temperature", "Soil temperature (85cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth95", "Soil Temperature", "Soil temperature (95cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth105", "Soil Temperature", "Soil temperature (105cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth115", "Soil Temperature", "Soil temperature (115cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth125", "Soil Temperature", "Soil temperature (125cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth135", "Soil Temperature", "Soil temperature (135cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msoiltempdepth145", "Soil Temperature", "Soil temperature (145cm)", "degC", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mch4", "CH4 Emissions", "CH4 emissions (Total)", "kgC/m2/year", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mch4diff", "CH4 Emissions", "CH4 emissions (Diffusion)", "kgC/m2/year", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mch4plan", "CH4 Emissions", "CH4 emissions (Plant-mediated)", "kgC/m2/year", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mch4ebull", "CH4 Emissions", "CH4 emissions (Ebullition)", "kgC/m2/year", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_msnow", "Snow Depth", "Snow depth", "m", AggregationLevel.Gridcell);
        AddMonthlyOutput(builder, "file_mwtp", "Water Table Depth", "Water table depth", "m", AggregationLevel.Gridcell);
        AddOutput(builder, "file_mald", "Active Layer Depth", "Active layer depth",
            ModelConstants.MonthCols.Select(c => (c, "m")).Concat(
                [("MAXALD", "m")]
            ).ToArray(), AggregationLevel.Gridcell, TemporalResolution.Monthly);
        AddMonthlyOutput(builder, "file_mburned_area_out", "BLAZE Burned Area", "BLAZE Burned Area", "0-1", AggregationLevel.Gridcell);

        Definitions = builder.ToImmutable();
    }

    /// <summary>
    /// Get metadata for a specific output file type
    /// </summary>
    /// <param name="fileType">The type of output file (e.g., "lai" for lai.out).</param>
    /// <returns>Metadata about the output file structure, or null if not a known type</returns>
    /// <exception cref="InvalidOperationException">Thrown if no metadata is found for the specified type.</exception>
    public static OutputFileMetadata GetMetadata(string fileType)
    {
        if (Definitions.TryGetValue(fileType, out var quantity))
            return quantity;
        throw new InvalidOperationException($"Unable to find metadata for unknown output file type: {fileType}");
    }

    /// <summary>
    /// Get all known output file types.
    /// </summary>
    public static IEnumerable<string> GetAllFileTypes() => Definitions.Keys;

    /// <summary>
    /// Register metadata for an output file.
    /// </summary>
    /// <param name="builder">The dictionary builder.</param>
    /// <param name="fileType">The type of output file (e.g., "lai" for lai.out).</param>
    /// <param name="name">The name of the output file (e.g., "Leaf Area Index").</param>
    /// <param name="description">A description of the output file (e.g., "Annual Leaf Area Index").</param>
    /// <param name="layers">A list of pairs of (layer name, units).</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    private static void AddOutput(ImmutableDictionary<string, OutputFileMetadata>.Builder builder, string fileType, string name, string description, (string layer, string units)[] layers, AggregationLevel level, TemporalResolution resolution)
    {
        builder.Add(fileType, new OutputFileMetadata(
            fileName: fileType,
            name: name,
            description: description,
            layers: new StaticLayers(layers.ToDictionary(l => l.layer, l => new Unit(l.units)), level, resolution),
            level: level,
            resolution: resolution));
    }

    /// <summary>
    /// Register metadata for an output file.
    /// </summary>
    /// <param name="builder">The dictionary builder.</param>
    /// <param name="fileType">The type of output file (e.g., "lai" for lai.out).</param>
    /// <param name="name">The name of the output file (e.g., "Leaf Area Index").</param>
    /// <param name="description">A description of the output file (e.g., "Annual Leaf Area Index").</param>
    /// <param name="units">The units of all columns in the output file.</param>
    /// <param name="layers">A list of layer names.</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    private static void AddOutput(ImmutableDictionary<string, OutputFileMetadata>.Builder builder, string fileType, string name, string description, string units, string[] layers, AggregationLevel level, TemporalResolution resolution)
    {
        builder.Add(fileType, new OutputFileMetadata(
            fileName: fileType,
            name: name,
            description: description,
            layers: new StaticLayers(layers, new Unit(units), level, resolution),
            level: level,
            resolution: resolution));
    }

    /// <summary>
    /// Register metadata for an output file.
    /// </summary>
    /// <param name="builder">The dictionary builder.</param>
    /// <param name="fileType">The type of output file (e.g., "lai" for lai.out).</param>
    /// <param name="name">The name of the output file (e.g., "Leaf Area Index").</param>
    /// <param name="description">A description of the output file (e.g., "Annual Leaf Area Index").</param>
    /// <param name="units">The units of all columns in the output file (e.g., "m2/m2").</param>
    /// <param name="level">The level at which data is aggregated.</param>
    /// <param name="resolution">The temporal resolution of the data.</param>
    /// <remarks>
    /// Note: Monthly resolution is not supported for dynamic layers.
    /// </remarks>
    private static void AddPftOutput(ImmutableDictionary<string, OutputFileMetadata>.Builder builder, string fileType, string name, string description, string units, AggregationLevel level, TemporalResolution resolution)
    {
        builder.Add(fileType, new OutputFileMetadata(
            fileName: fileType,
            name: name,
            description: description,
            layers: new DynamicLayers(new Unit(units), level, resolution),
            level: level,
            resolution: resolution));
    }

    /// <summary>
    /// Register metadata for a monthly output file.
    /// </summary>
    /// <param name="builder">The dictionary builder.</param>
    /// <param name="fileType">The type of output file (e.g., "lai" for lai.out).</param>
    /// <param name="name">The name of the output file (e.g., "Leaf Area Index").</param>
    /// <param name="description">A description of the output file (e.g., "Annual Leaf Area Index").</param>
    /// <param name="units">The units of all columns in the output file (e.g., "m2/m2").</param>
    /// <param name="level">The level at which data is aggregated.</param>
    private static void AddMonthlyOutput(ImmutableDictionary<string, OutputFileMetadata>.Builder builder, string fileType, string name, string description, string units, AggregationLevel level)
    {
        builder.Add(fileType, new OutputFileMetadata(
            fileName: fileType,
            name: name,
            description: description,
            layers: new StaticLayers(ModelConstants.MonthCols, new Unit(units), level, TemporalResolution.Monthly),
            level: level,
            resolution: TemporalResolution.Monthly));
    }
}
