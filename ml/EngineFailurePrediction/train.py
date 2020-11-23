import pip

def install(package):
    if hasattr(pip, 'main'):
        pip.main(['install', package])
    else:
        pip._internal.main(['install', package])

# Install OpenCV correctly
install('opencv-python')


# General references
import argparse
import os
import numpy as np
import pandas as pd
import joblib

# Add arcus references
from arcus.ml import dataframes as adf
from arcus.ml.timeseries import timeops
#from arcus.ml.images import *
from arcus.ml.evaluation import classification as clev
from arcus.azureml.environment.aml_environment import AzureMLEnvironment
from arcus.azureml.experimenting.aml_trainer import AzureMLTrainer

# Add AzureML references
from azureml.core import Workspace, Dataset, Datastore, Experiment, Run
from azureml.core import VERSION

# This section enables to use the module code referenced in the repo
import os
import os.path
import sys
import time
from datetime import date

import math
import matplotlib
import matplotlib.pyplot as plt
import seaborn as sns
from collections import Counter

from sklearn.model_selection import train_test_split
import sklearn.metrics as metrics
from sklearn.preprocessing import MinMaxScaler

# Tensorflow / Keras references.  Feel free to remove when not used
import tensorflow as tf
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Activation
from tensorflow.keras.optimizers import SGD
from tensorflow.keras.layers import Dense, Dropout, Flatten
from tensorflow.keras.layers import Conv2D, MaxPooling2D
from tensorflow.keras.layers import Input, Dense, Conv2D, MaxPooling2D, UpSampling2D
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Flatten, Bidirectional, GRU, Activation, BatchNormalization,SpatialDropout1D,Bidirectional, Embedding, LSTM
from tensorflow.keras.layers import Conv2D, MaxPooling2D
from tensorflow.keras.models import Model
from tensorflow.keras.preprocessing import image
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Dropout, Activation, BatchNormalization
from tensorflow.keras.applications.vgg19 import VGG19
from tensorflow.keras.applications.vgg19 import preprocess_input, decode_predictions
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.utils import to_categorical
from tensorflow.keras.losses import categorical_crossentropy, binary_crossentropy, cosine_similarity
from tensorflow.keras.utils import to_categorical
from tensorflow.keras import backend as K
from tensorflow.keras.callbacks import EarlyStopping, ModelCheckpoint


##########################################
### Parse arguments and prepare environment
##########################################

parser = argparse.ArgumentParser()

# If you want to parse arguments that get passed through the estimator, this can be done here
parser.add_argument('--epochs', type=int, dest='epochs', default=10, help='Epoch count')
parser.add_argument('--dropout', type=int, dest='dropout', default=0, help='Dropout percentage')
parser.add_argument('--lstmnodes', type=int, dest='lstmnodes', default=50, help='LSTM node count')
parser.add_argument('--batch_size', type=int, dest='batch_size', default=32, help='Batch size')
parser.add_argument('--es_patience', type=int, dest='es_patience', default=-1, help='Early stopping patience. If less than zero, no Early stopping')
parser.add_argument('--train_test_split_ratio', type=float, dest='train_test_split_ratio', default=0.3, help='Train test split ratio')

args, unknown = parser.parse_known_args()
epoch_count = args.epochs
dropout = args.dropout / 100
lstm_nodes = args.lstmnodes
batch_size = args.batch_size
es_patience = args.es_patience
train_test_split_ratio = args.train_test_split_ratio

# Load the environment from the Run context, so you can access any dataset
aml_environment = AzureMLEnvironment.CreateFromContext()
trainer = AzureMLTrainer.CreateFromContext()

if not os.path.exists('outputs'):
    os.makedirs('outputs')

##########################################
### Access datasets
##########################################

# Access tabular dataset (which is not passed as input)
dataset = aml_environment.load_tabular_dataset('engine-data')

# Adding 'failure' column
dataset['failure'] = dataset.apply(lambda row: 0 if row.ttf > 50 else 1, axis = 1)

# Removing output features and irrelevant features
dataset.drop('ttf', axis=1, inplace=True)
dataset.drop('cycle', axis=1, inplace=True) 

# Taking datasets and extract training/test data
seq_length = 30
train_df = dataset[dataset.engine_id <= 75]
test_df = dataset[dataset.engine_id > 75]

print(f'Training samples: {len(train_df)}')
print(f'Test samples: {len(test_df)}')




def build_lstm(dropout = 0.0, lstm_nodes: int = 20, bi_directional: bool = False):
    model = Sequential()
    data_shape = (X_train.shape[1], X_train.shape[2])
    model.add(LSTM(lstm_nodes, input_shape=data_shape, activation = 'relu', return_sequences = True))
    model.add(Dropout(dropout))
    
    if(bi_directional):
        model.add(Bidirectional(LSTM(30, activation='relu')))
        model.add(Dropout(dropout))
    else:
        model.add(GRU(30))
    
    model.add(Dense(1, activation='sigmoid'))
    model.compile(loss = ('binary_crossentropy'), optimizer='adam', metrics=['acc'])
    return model


def get_windows(sorted_df: pd.DataFrame, window_size: int, window_stride: int = 1, group_column: str = None, zero_padding: bool = False, remove_group_column: bool = False, target_column: str = None) -> np.array:
    if group_column is None:
        return __get_windows_from_group(sorted_df, window_size, window_stride, zero_padding, target_column)

    else:
        windows = None
        targets = None
        
        # create unique list of groups
        _groups = sorted_df[group_column].unique()

        for key in _groups:
            _group_df = sorted_df[:][sorted_df[group_column] == key]
            if(remove_group_column):
                _group_df.drop(group_column, axis=1, inplace=True)
            _current_windows, _current_targets = __get_windows_from_group(
                _group_df, window_size, window_stride, zero_padding, target_column)
            if windows is None:
                windows = _current_windows
            else:
                windows = np.concatenate((windows, _current_windows))

            if target_column is not None:
                if targets is None:
                    targets = _current_targets
                else:
                    targets = np.concatenate((targets, _current_targets))

        return windows, targets

def __get_windows_from_group(sorted_df: pd.DataFrame, window_size: int, 
                             window_stride: int = 1, zero_padding: bool = False,
                             target_column:str = None) -> np.array:
    # zero based row to take the leading window from - this row will be last row in the window
    _start_row_idx = 0 if zero_padding else window_size - 1
    # will contain all windows
    windows = list()
    targets = list()
    
    # range will be from start_row to row_count
    for __current_row_idx in range(_start_row_idx, len(sorted_df)):
        __slice_begin_idx = (__current_row_idx - window_size +
                             1) if __current_row_idx >= window_size else 0
        window_df = sorted_df.copy()
        time_slice = window_df.iloc[__slice_begin_idx:__current_row_idx + 1, :]
        time_array = np.array(time_slice.values)
        if(zero_padding):
            _rows_to_pad = window_size - __current_row_idx - 1
            if(_rows_to_pad > 0):
                padding_matrix = np.zeros(
                    (_rows_to_pad, len(sorted_df.columns)))
                time_array = np.concatenate((padding_matrix, time_array))
        if(target_column is not None):
            _target_colidx = sorted_df.columns.get_loc(target_column)
            time_array, target_array = __pop_from_array(time_array, _target_colidx)
            targets.append(target_array)

        scaler = MinMaxScaler().fit(time_array) 

        time_array = scaler.transform(time_array)
        windows.append(time_array)
    return np.array(windows), np.array(targets) if target_column is not None else None

def __pop_from_array(my_array,pc):
    i = pc
    pop = my_array[:,i]
    new_array = np.hstack((my_array[:,:i],my_array[:,i+1:]))
    return new_array, pop

##########################################
### Perform training
##########################################

# Load data 
X_train, y_train = get_windows(train_df, seq_length, group_column='engine_id', zero_padding=True, target_column='failure', remove_group_column=True)
X_test, y_test = get_windows(test_df, seq_length, group_column='engine_id', zero_padding=True, target_column='failure', remove_group_column=True)

# Only keeping the last values of the target arrays
y_train = y_train[:, -1]
y_test = y_test[:, -1]


# Build model 
model = build_lstm(dropout = dropout, lstm_nodes=lstm_nodes, bi_directional=False)

early_stopping = EarlyStopping(monitor='val_loss', mode='min', verbose=1, patience=5)
tensorboard_callback = tf.keras.callbacks.TensorBoard('./tensor_logs', histogram_freq=1),
model_checkpoint_callback = ModelCheckpoint(
    filepath='outputs/model_checkpoint',
    save_weights_only=False,
    monitor='val_acc',
    mode='max',
    save_best_only=True)
cbs =[early_stopping, *tensorboard_callback, model_checkpoint_callback]

#_run = aml_trainer.new_run(copy_folder = True, metrics = {'epochs': epoch_count, 'batch_size': batch_size, 'lstm_nodes': lstm_nodes, 'dropout': dropout})

model = build_lstm(dropout = dropout, lstm_nodes=lstm_nodes, bi_directional=False)

# shuffle data prior to training
model.fit(X_train, y_train,
                epochs=epoch_count,
                batch_size=batch_size,
                validation_split = 0.2,
                callbacks = cbs)

# Custom metrics tracking
# trainer._log_metrics('dice_coef_loss', list(fitted_model.history.history['dice_coef_loss'])[-1], description='')

trainer.evaluate_classifier(model, X_test, y_test, show_roc = True, upload_model = False)   

##########################################
### Save model
##########################################
model.save('outputs/model')

print('Training finished')