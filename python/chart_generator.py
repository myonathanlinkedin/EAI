#!/usr/bin/env python3
"""
EAI System Chart Generator
Unified script to generate compelling visualizations from benchmark results
Creates both business presentation charts and academic paper-ready figures
"""

import json
import os
import matplotlib.pyplot as plt
import seaborn as sns
import pandas as pd
import numpy as np
from datetime import datetime
import plotly.graph_objects as go
import plotly.express as px
from plotly.subplots import make_subplots
import matplotlib.patches as patches
from matplotlib.patches import FancyBboxPatch
import warnings
warnings.filterwarnings('ignore')

class EAIChartGenerator:
    def __init__(self, benchmark_file=None):
        """Initialize with benchmark data"""
        if benchmark_file is None:
            # Find the latest benchmark file in parent directory
            parent_dir = '..'
            benchmark_files = [f for f in os.listdir(parent_dir) if f.startswith('benchmark_report_') and f.endswith('.json')]
            if benchmark_files:
                benchmark_file = os.path.join(parent_dir, sorted(benchmark_files)[-1])  # Get the latest
            else:
                raise FileNotFoundError("No benchmark report found!")
        
        self.benchmark_file = benchmark_file
        with open(benchmark_file, 'r') as f:
            self.data = json.load(f)
        
        self.timestamp = self.data['Timestamp']
        self.environment = self.data['Environment']
        self.results = self.data['Results']
        self.summary = self.results['Summary']
        
        # Create single output directory
        self.charts_dir = 'charts'
        os.makedirs(self.charts_dir, exist_ok=True)
        
        print(f"Loaded benchmark data from {benchmark_file}")
        print(f"Model: {self.environment['Model']}")
        print(f"Total Requests: {self.summary['TotalRequests']}")
        print(f"Success Rate: {self.summary['SuccessRate']}%")

    def create_performance_dashboard(self):
        """Create 3D performance dashboard"""
        fig = plt.figure(figsize=(16, 12))
        fig.suptitle('EAI System Performance Dashboard', fontsize=16, fontweight='bold', y=0.98)
        
        # Get data
        load_test_results = self.results['LoadTestResults']
        response_times = [r['ResponseTime'] / 1000 for r in load_test_results]
        confidences = [r['Confidence'] for r in load_test_results if r['Confidence'] > 0]
        decisions = [r['Decision'] for r in load_test_results]
        
        # Create 3D subplot
        ax = fig.add_subplot(111, projection='3d')
        
        # Create 3D scatter plot: Response Time vs Confidence vs Decision Type
        decision_colors = {'escalate': 'red', 'approve': 'green', 'deny': 'orange'}
        decision_markers = {'escalate': 'o', 'approve': 's', 'deny': '^'}
        
        # Get unique decisions for legend
        unique_decisions = list(set(decisions))
        
        for i, (time, conf, decision) in enumerate(zip(response_times, confidences, decisions)):
            ax.scatter(time, conf, i, 
                      c=decision_colors.get(decision, 'blue'), 
                      marker=decision_markers.get(decision, 'o'),
                      s=100, alpha=0.7, edgecolors='black', linewidth=1)
        
        ax.set_xlabel('Response Time (seconds)', fontweight='bold', fontsize=12)
        ax.set_ylabel('Confidence Score', fontweight='bold', fontsize=12)
        ax.set_zlabel('Request Sequence', fontweight='bold', fontsize=12)
        ax.set_title('3D Performance Analysis: Time vs Confidence vs Request Order', 
                    fontweight='bold', fontsize=14, pad=30)
        
        # Add legend - only show decisions that actually exist in data
        legend_elements = []
        for decision in unique_decisions:
            color = decision_colors.get(decision, 'blue')
            marker = decision_markers.get(decision, 'o')
            count = decisions.count(decision)
            legend_elements.append(
                plt.Line2D([0], [0], marker=marker, color='w', markerfacecolor=color, 
                          markersize=10, label=f'{decision.title()} Decision ({count})')
            )
        
        # Move legend to bottom
        ax.legend(handles=legend_elements, loc='lower center', bbox_to_anchor=(0.5, -0.15), 
                 ncol=len(legend_elements), fontsize=10)
        
        # Add performance metrics as text
        metrics_text = f"""Performance Metrics:
• Total Requests: {self.summary['TotalRequests']}
• Success Rate: {self.summary['SuccessRate']}%
• Avg Response Time: {self.summary['AverageResponseTime']/1000:.1f}s
• Avg Confidence: {self.summary['AverageConfidence']:.2f}
• Model: {self.environment['Model']}"""
        
        ax.text2D(0.02, 0.98, metrics_text, transform=ax.transAxes, 
                 fontsize=10, verticalalignment='top',
                 bbox=dict(boxstyle="round,pad=0.5", facecolor="lightblue", alpha=0.8))
        
        plt.tight_layout()
        plt.subplots_adjust(top=0.95, bottom=0.15)
        plt.savefig(f'{self.charts_dir}/performance_dashboard_3d.png', dpi=300, bbox_inches='tight')
        plt.close()
        print("Created 3D performance dashboard")

    def create_business_impact_chart(self):
        """Create business impact analysis chart using only real benchmark data"""
        fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(15, 12))
        fig.suptitle('EAI System Business Impact Analysis', fontsize=12, fontweight='bold', y=0.98)
        
        # Use only real benchmark data
        avg_response_time_seconds = self.summary['AverageResponseTime'] / 1000
        success_rate = self.summary['SuccessRate']
        total_requests = self.summary['TotalRequests']
        avg_confidence = self.summary['AverageConfidence']
        
        # 1. Response Time Analysis (Real Data)
        load_test_results = self.results['LoadTestResults']
        response_times = [r['ResponseTime'] / 1000 for r in load_test_results]  # Convert to seconds
        
        ax1.hist(response_times, bins=6, alpha=0.7, color='skyblue', edgecolor='black')
        ax1.axvline(np.mean(response_times), color='red', linestyle='--', linewidth=2,
                   label=f'Mean: {np.mean(response_times):.1f}s')
        ax1.set_xlabel('Response Time (seconds)')
        ax1.set_ylabel('Frequency')
        ax1.set_title('Response Time Distribution (Real Data)', pad=25)
        ax1.legend()
        ax1.grid(True, alpha=0.3)
        
        # 2. Success Rate Analysis (Real Data)
        success_metrics = {
            'Total Requests': total_requests,
            'Successful Requests': self.summary['SuccessfulRequests'],
            'Failed Requests': self.summary['FailedRequests'],
            'Success Rate': success_rate
        }
        
        bars = ax2.bar(success_metrics.keys(), success_metrics.values(), 
                      color=['blue', 'green', 'red', 'orange'], alpha=0.8)
        ax2.set_ylabel('Count / Percentage')
        ax2.set_title('Request Success Analysis (Real Data)', pad=25)
        ax2.grid(True, alpha=0.3)
        
        # Add value labels
        for bar, value in zip(bars, success_metrics.values()):
            height = bar.get_height()
            ax2.text(bar.get_x() + bar.get_width()/2., height + 0.1,
                    f'{value}', ha='center', va='bottom', fontweight='bold')
        
        # 3. Decision Distribution (Real Data)
        decision_counts = self.summary['DecisionDistribution']
        labels = list(decision_counts.keys())
        sizes = list(decision_counts.values())
        colors = ['#ff6b6b', '#4ecdc4', '#45b7d1']
        
        wedges, texts, autotexts = ax3.pie(sizes, labels=labels, autopct='%1.1f%%',
                                          colors=colors[:len(labels)], startangle=90)
        ax3.set_title('Decision Distribution (Real Data)', pad=25)
        
        # 4. Performance Metrics Summary (Real Data)
        metrics = ['Min Response Time', 'Avg Response Time', 'Max Response Time', 'Avg Confidence']
        values = [
            self.summary['MinResponseTime'] / 1000,
            self.summary['AverageResponseTime'] / 1000,
            self.summary['MaxResponseTime'] / 1000,
            self.summary['AverageConfidence']
        ]
        colors = ['lightgreen', 'lightblue', 'lightcoral', 'lightyellow']
        
        bars = ax4.bar(metrics, values, color=colors, alpha=0.8, edgecolor='black')
        ax4.set_ylabel('Time (seconds) / Confidence Score')
        ax4.set_title('Performance Metrics Summary (Real Data)', pad=25)
        ax4.grid(True, alpha=0.3)
        
        # Add value labels
        for bar, value in zip(bars, values):
            height = bar.get_height()
            ax4.text(bar.get_x() + bar.get_width()/2., height + 0.01,
                    f'{value:.2f}', ha='center', va='bottom', fontweight='bold')
        
        plt.tight_layout()
        plt.subplots_adjust(top=0.95)
        plt.savefig(f'{self.charts_dir}/business_impact_analysis.png', dpi=300, bbox_inches='tight')
        plt.close()
        print("Created business impact analysis (real data only)")

    def create_3d_response_analysis(self):
        """Create 3D response time analysis"""
        fig = plt.figure(figsize=(16, 12))
        fig.suptitle('EAI System 3D Response Time Analysis', fontsize=16, fontweight='bold', y=0.98)
        
        # Get data
        load_test_results = self.results['LoadTestResults']
        response_times = [r['ResponseTime'] / 1000 for r in load_test_results]
        confidences = [r['Confidence'] for r in load_test_results if r['Confidence'] > 0]
        
        # Create 3D subplot
        ax = fig.add_subplot(111, projection='3d')
        
        # Create 3D scatter plot
        x = np.array(response_times)
        y = np.array(confidences)
        z = np.arange(len(response_times))
        
        # Create 3D scatter plot
        scatter = ax.scatter(x, y, z, c=z, cmap='viridis', s=150, alpha=0.8, edgecolors='black')
        
        # Add lines connecting points
        ax.plot(x, y, z, 'k-', alpha=0.3, linewidth=1)
        
        ax.set_xlabel('Response Time (seconds)', fontweight='bold', fontsize=12)
        ax.set_ylabel('Confidence Score', fontweight='bold', fontsize=12)
        ax.set_zlabel('Request Sequence', fontweight='bold', fontsize=12)
        ax.set_title('3D Response Time vs Confidence Analysis', 
                    fontweight='bold', fontsize=12, pad=40)
        
        # Add colorbar
        cbar = plt.colorbar(scatter, ax=ax, shrink=0.5, aspect=20)
        cbar.set_label('Request Order', fontweight='bold')
        
        # Add legend at bottom
        legend_text = f"""Legend:
• X-axis: Response Time (seconds) - Shows processing speed
• Y-axis: Confidence Score - Shows decision certainty  
• Z-axis: Request Sequence - Shows chronological order
• Color: Request Order - Darker = earlier requests
• Surface: Shows relationship between time and confidence

Performance Summary:
• Total Requests: {self.summary['TotalRequests']}
• Success Rate: {self.summary['SuccessRate']}%
• Avg Response Time: {self.summary['AverageResponseTime']/1000:.1f}s
• Avg Confidence: {self.summary['AverageConfidence']:.2f}"""
        
        fig.text(0.5, 0.02, legend_text, ha='center', va='bottom', fontsize=10,
                bbox=dict(boxstyle="round,pad=0.5", facecolor="lightyellow", alpha=0.8))
        
        plt.tight_layout()
        plt.subplots_adjust(top=0.95, bottom=0.25)
        plt.savefig(f'{self.charts_dir}/response_time_analysis_3d.png', dpi=300, bbox_inches='tight')
        plt.close()
        print("Created 3D response time analysis")

    def create_3d_decision_analysis(self):
        """Create 3D decision analysis"""
        fig = plt.figure(figsize=(16, 12))
        fig.suptitle('EAI System 3D Decision Analysis', fontsize=16, fontweight='bold', y=0.98)
        
        # Get data
        load_test_results = self.results['LoadTestResults']
        response_times = [r['ResponseTime'] / 1000 for r in load_test_results]
        confidences = [r['Confidence'] for r in load_test_results if r['Confidence'] > 0]
        decisions = [r['Decision'] for r in load_test_results]
        
        # Create 3D subplot
        ax = fig.add_subplot(111, projection='3d')
        
        # Create 3D bar chart for decision distribution
        decision_counts = self.summary['DecisionDistribution']
        decision_names = list(decision_counts.keys())
        decision_values = list(decision_counts.values())
        
        # Create 3D bars
        x_pos = np.arange(len(decision_names))
        y_pos = np.zeros(len(decision_names))
        z_pos = np.zeros(len(decision_names))
        dx = np.ones(len(decision_names)) * 0.8
        dy = np.ones(len(decision_names)) * 0.8
        dz = decision_values
        
        colors = ['red', 'green', 'blue', 'orange']
        bars = ax.bar3d(x_pos, y_pos, z_pos, dx, dy, dz, 
                       color=colors[:len(decision_names)], alpha=0.7, edgecolor='black')
        
        ax.set_xlabel('Decision Types', fontweight='bold', fontsize=12)
        ax.set_ylabel('Y Position', fontweight='bold', fontsize=12)
        ax.set_zlabel('Number of Decisions', fontweight='bold', fontsize=12)
        ax.set_title('3D Decision Distribution Analysis', 
                    fontweight='bold', fontsize=12, pad=40)
        
        # Set x-axis labels
        ax.set_xticks(x_pos + 0.4)
        ax.set_xticklabels(decision_names)
        
        # Add legend at bottom
        legend_text = f"""Legend:
• X-axis: Decision Types - Shows different decision categories
• Y-axis: Y Position - Spatial positioning for 3D effect
• Z-axis: Number of Decisions - Shows frequency of each decision type
• Colors: Red = Escalate, Green = Approve, Blue = Other decisions

Decision Analysis:
• Total Decisions: {sum(decision_values)}
• Escalate: {decision_counts.get('escalate', 0)} decisions
• Approve: {decision_counts.get('approve', 0)} decisions
• Success Rate: {self.summary['SuccessRate']}%
• Avg Confidence: {self.summary['AverageConfidence']:.2f}

Model: {self.environment['Model']}"""
        
        fig.text(0.5, 0.02, legend_text, ha='center', va='bottom', fontsize=10,
                bbox=dict(boxstyle="round,pad=0.5", facecolor="lightgreen", alpha=0.8))
        
        plt.tight_layout()
        plt.subplots_adjust(top=0.95, bottom=0.25)
        plt.savefig(f'{self.charts_dir}/decision_analysis_3d.png', dpi=300, bbox_inches='tight')
        plt.close()
        print("Created 3D decision analysis")

    def create_decision_analysis_chart(self):
        """Create decision making analysis chart using only real benchmark data"""
        fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(15, 12))
        fig.suptitle('EAI Decision Making Analysis', fontsize=12, fontweight='bold', y=0.98)
        
        # Get decision data from real benchmark
        load_test_results = self.results['LoadTestResults']
        confidences = [r['Confidence'] for r in load_test_results if r['Confidence'] > 0]
        decisions = [r['Decision'] for r in load_test_results]
        decision_counts = self.summary['DecisionDistribution']
        
        # Subplot 1: Decision Distribution (Real Data)
        labels = list(decision_counts.keys())
        sizes = list(decision_counts.values())
        colors = ['#ff6b6b', '#4ecdc4', '#45b7d1']
        
        wedges, texts, autotexts = ax1.pie(sizes, labels=labels, autopct='%1.1f%%',
                                          colors=colors[:len(labels)], startangle=90)
        ax1.set_title('Decision Distribution (Real Data)', pad=25)
        
        # Subplot 2: Confidence Score Distribution (Real Data)
        ax2.hist(confidences, bins=6, alpha=0.7, color='lightgreen', 
                edgecolor='black', linewidth=1)
        mean_conf = np.mean(confidences)
        ax2.axvline(mean_conf, color='red', linestyle='--', linewidth=2,
                   label=f'Mean: {mean_conf:.2f}')
        ax2.set_xlabel('Confidence Score')
        ax2.set_ylabel('Frequency')
        ax2.set_title('Confidence Score Distribution (Real Data)', pad=25)
        ax2.legend()
        ax2.grid(True, alpha=0.3)
        
        # Subplot 3: Confidence by Decision Type (Real Data)
        decision_confidences = {}
        for result in load_test_results:
            decision = result['Decision']
            confidence = result['Confidence']
            if confidence > 0:
                if decision not in decision_confidences:
                    decision_confidences[decision] = []
                decision_confidences[decision].append(confidence)
        
        if decision_confidences:
            decisions_list = list(decision_confidences.keys())
            confidences_list = list(decision_confidences.values())
            
            bp = ax3.boxplot(confidences_list, labels=decisions_list, patch_artist=True)
            colors_box = ['lightcoral', 'lightblue', 'lightgreen']
            for patch, color in zip(bp['boxes'], colors_box[:len(decisions_list)]):
                patch.set_facecolor(color)
                patch.set_alpha(0.8)
            
            ax3.set_ylabel('Confidence Score')
            ax3.set_title('Confidence Score by Decision Type (Real Data)', pad=25)
            ax3.grid(True, alpha=0.3)
        
        # Subplot 4: System Performance Summary (Real Data)
        performance_metrics = {
            'Success Rate (%)': self.summary['SuccessRate'],
            'Avg Confidence': self.summary['AverageConfidence'] * 100,
            'Total Requests': self.summary['TotalRequests'],
            'Avg Response Time (s)': self.summary['AverageResponseTime'] / 1000
        }
        
        bars = ax4.bar(performance_metrics.keys(), performance_metrics.values(), 
                      color=['green', 'blue', 'orange', 'purple'], alpha=0.8)
        ax4.set_ylabel('Value')
        ax4.set_title('System Performance Summary (Real Data)', pad=25)
        ax4.grid(True, alpha=0.3)
        
        # Add value labels
        for bar, value in zip(bars, performance_metrics.values()):
            height = bar.get_height()
            ax4.text(bar.get_x() + bar.get_width()/2., height + 0.1,
                    f'{value:.1f}', ha='center', va='bottom', fontweight='bold')
        
        plt.tight_layout()
        plt.subplots_adjust(top=0.95)  # Add extra space for main title
        plt.savefig(f'{self.charts_dir}/decision_analysis.png', dpi=300, bbox_inches='tight')
        plt.close()
        print("Created decision analysis chart (real data only)")

    def generate_all_charts(self):
        """Generate all charts"""
        print("Starting EAI Chart Generation...")
        print(f"Charts directory: {self.charts_dir}")
        
        try:
            # Generate 3D charts
            self.create_performance_dashboard()
            self.create_3d_response_analysis()
            self.create_3d_decision_analysis()
            self.create_business_impact_chart()
            self.create_decision_analysis_chart()
            
            print("\nAll charts generated successfully!")
            print(f"Charts saved in: {os.path.abspath(self.charts_dir)}")
            
            print("\nGenerated files:")
            if os.path.exists(self.charts_dir):
                files = os.listdir(self.charts_dir)
                print(f"\n{self.charts_dir}/ ({len(files)} files):")
                for file in files:
                    print(f"  {file}")
                
        except Exception as e:
            print(f"Error generating charts: {e}")
            raise

def main():
    """Main function to generate all charts"""
    try:
        generator = EAIChartGenerator()
        generator.generate_all_charts()
        
        print("\n" + "="*60)
        print("CHART GENERATION SUMMARY")
        print("="*60)
        print(f"Model: {generator.environment['Model']}")
        print(f"Total Requests: {generator.summary['TotalRequests']}")
        print(f"Success Rate: {generator.summary['SuccessRate']}%")
        print(f"Avg Response Time: {generator.summary['AverageResponseTime']/1000:.1f}s")
        print(f"Avg Confidence: {generator.summary['AverageConfidence']:.2f}")
        print(f"Charts Generated: 5 comprehensive visualizations")
        print("="*60)
        
    except Exception as e:
        print(f"Failed to generate charts: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())
