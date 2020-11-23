// creates a N x 18 size array
function createTimeWindows(engine_windows) {
    'use strict';
    var result = [];
    var output = [];

    for(var window_id in engine_windows){
        var array = [];
        array.push(engine_windows[window_id].value.setting1);
        array.push(engine_windows[window_id].value.setting2);
        array.push(engine_windows[window_id].value.setting3);
        array.push(engine_windows[window_id].value.temp_fan_inlet);
        array.push(engine_windows[window_id].value.temp_lpc_outlet);
        array.push(engine_windows[window_id].value.temp_hpc_outlet);
        array.push(engine_windows[window_id].value.temp_lpt_outlet);
        array.push(engine_windows[window_id].value.pressure_fan_inlet);
        array.push(engine_windows[window_id].value.pressure_bypass_duct);
        array.push(engine_windows[window_id].value.pressure_hpc_outlet);
        array.push(engine_windows[window_id].value.physical_fan_speed);
        array.push(engine_windows[window_id].value.physical_core_speed);
        array.push(engine_windows[window_id].value.engine_pressure_ratio);
        array.push(engine_windows[window_id].value.static_pressure_hpc_outlet);
        array.push(engine_windows[window_id].value.fuel_flow_ration_ps30);
        array.push(engine_windows[window_id].value.corrected_fan_speed);
        array.push(engine_windows[window_id].value.corrected_core_speed);
        array.push(engine_windows[window_id].value.bypass_ratio);
        array.push(engine_windows[window_id].value.burner_fuel_air_ratio);
        array.push(engine_windows[window_id].value.bleed_enthalpy);
        array.push(engine_windows[window_id].value.demanded_fan_speed);
        array.push(engine_windows[window_id].value.demanded_corrected_fan_speed);
        array.push(engine_windows[window_id].value.hpt_collant_bleed);
        array.push(engine_windows[window_id].value.lpt_coolant_bleed);
        output.push(array);
    }
    result.push(output);
    return result;
    //return output;
}    